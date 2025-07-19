import re
import sys

# Regex pattern equivalent to the original JS version

guid_pattern = r'\{fileID: .*(\n\s+type: [0-9]+)?\}'
pattern = re.compile(
    "".join([
        fr"    - target: {guid_pattern}\n",
        r"      propertyPath: (serializedProgramAsset|serializationData\..*)\n",
        r"      value:.*\n",
        fr"      objectReference: {guid_pattern}\n",
        r"|",
        fr"  serialized(Udon)?ProgramAsset: {guid_pattern}\n",
        r"|",
        r"    SerializedFormat: [02]\n",
    ]),
    flags=re.MULTILINE
)

# Read from file or stdin
if len(sys.argv) > 1:
    with open(sys.argv[1], "r", encoding="utf-8") as f:
        content = f.read()
else:
    content = sys.stdin.read()

# Apply the filter
cleaned = pattern.sub("", content)

# Output to stdout
sys.stdout.write(cleaned)
