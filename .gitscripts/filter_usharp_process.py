#!/usr/bin/env python3
import re
import sys
import traceback

# -------- Regex Setup --------

GUID_PATTERN = r'\{fileID: .*(\r?\n\s+type: [0-9]+)?\}'
pattern = re.compile(
    "".join([
        fr"    - target: {GUID_PATTERN}\r?\n",
        r"      propertyPath: (serializedProgramAsset|serializationData\..*)\r?\n",
        r"      value:.*\r?\n",
        fr"      objectReference: {GUID_PATTERN}\r?\n",
        r"|",
        fr"  serialized(Udon)?ProgramAsset: {GUID_PATTERN}\r?\n",
        r"|",
        r"    SerializedFormat: [02]\r?\n",
    ]),
    flags=re.MULTILINE
)

# -------- Logging --------

DEBUG = False

def log(msg: str) -> None:
    """Log messages to stderr if DEBUG is enabled."""
    if DEBUG:
        print(f"[usharp-filter-process] {msg}", file=sys.stderr, flush=True)

# -------- Git pkt-line helpers --------

def read_pkt_line() -> str | None:
    """Read a single pkt-line from stdin."""
    try:
        length_str = sys.stdin.buffer.read(4)
        if not length_str or len(length_str) < 4:
            return None
        length = int(length_str, 16)
        if length == 0:
            return None  # flush packet
        data = sys.stdin.buffer.read(length - 4)
        return data.decode("utf-8", errors="replace")
    except Exception as e:      # pylint: disable=broad-exception-caught
        log(f"Error reading pkt-line: {e}")
        return None

MAX_PAYLOAD = 65516 # Maximum payload size for pkt-lines

def write_pkt_data(data: str) -> None:
    """Write potentially large data by chunking into valid pkt-lines."""
    encoded = data.encode("utf-8")
    start = 0
    while start < len(encoded):
        chunk = encoded[start:start + MAX_PAYLOAD]
        sys.stdout.buffer.write(f"{len(chunk)+4:04x}".encode("ascii"))
        sys.stdout.buffer.write(chunk)
        start += MAX_PAYLOAD
    sys.stdout.flush()

def write_pkt_line(s: str) -> None:
    """Write a single short pkt-line (for key=value control messages)."""
    payload = s.encode("utf-8")
    if len(payload) > MAX_PAYLOAD:
        raise ValueError(f"Control pkt-line too long: {len(payload)} bytes")
    sys.stdout.buffer.write(f"{len(payload)+4:04x}".encode("ascii"))
    sys.stdout.buffer.write(payload)
    sys.stdout.flush()

def write_flush() -> None:
    """Write a flush pkt-line to signal end of data."""
    try:
        sys.stdout.buffer.write(b"0000")
        sys.stdout.flush()
    except BrokenPipeError:
        sys.exit(0)

def read_until_flush() -> list[str]:
    """Read lines from stdin until a flush pkt-line is encountered."""
    lines = []
    while True:
        line = read_pkt_line()
        if line is None:
            break
        # log(f"Received line of length {len(line)}")
        lines.append(line)
    return lines

# -------- Handshake --------

def handshake() -> None:
    """Perform the initial handshake with Git."""

    welcome_lines = read_until_flush()
    if not welcome_lines or "git-filter-client\n" not in welcome_lines[0]:
        log("Did not receive expected welcome line from Git.")
        log(f"Received: {welcome_lines}")
        sys.exit(1)
    if "version=2\n" not in welcome_lines:
        log("Git filter version not supported.")
        sys.exit(1)

    write_pkt_line("git-filter-server")
    write_pkt_line("version=2")
    write_flush()

    capabilities = read_until_flush()
    if "capability=clean\n" not in capabilities:
        log("Git did not request 'clean' capability.")
        sys.exit(1)

    write_pkt_line("capability=clean")
    write_flush()

# -------- Main --------

def main() -> None:
    """Main processing loop for the Git filter."""
    log("Starting uSharp filter process...")

    handshake()

    log("Handshake complete. Ready to process commands.")

    while True:
        try:
            header_lines = read_until_flush()
            if not header_lines:
                log("EOF from Git — exiting.")
                break

            command = None
            pathname = None

            for line in header_lines:
                if line.startswith("command="):
                    command = line.split("=", 1)[1].strip()
                elif line.startswith("pathname="):
                    pathname = line.split("=", 1)[1].strip()

            log(f"Received command: {command}, pathname: {pathname}")

            content_lines = read_until_flush()
            content = "".join(content_lines)

            log(f"Content length: {len(content)} bytes")

            if command == "clean":
                log(f"Cleaning: {pathname}")
                cleaned = pattern.sub("", content)

                write_pkt_line("status=success\n")
                write_flush()

                if cleaned:
                    # for line in cleaned.splitlines(keepends=True):
                    write_pkt_data(cleaned)
                write_flush()

                write_flush()   # empty list, keep "status=success" unchanged!

                log(f"Cleaned content length: {len(cleaned)} bytes")

            else:
                log(f"Unknown command: {command}")
                write_pkt_line("status=error\n")
                write_flush()

                log("Skipping processing for unknown command.")
                break

        except Exception as e:      # pylint: disable=broad-exception-caught
            log(f"Fatal error in main loop: {e}")
            log(traceback.format_exc())
            write_pkt_line("status=abort\n")
            write_flush()
            break

if __name__ == "__main__":
    main()
