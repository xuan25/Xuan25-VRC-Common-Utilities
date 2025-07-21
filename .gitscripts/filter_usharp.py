import re
import sys

# This script filters out U# serialized data from Unity YAML files.

# pattern to match U# serialized data
guid_pattern = r'\{fileID: .*(\r?\n\s+type: [0-9]+)?\}'
pattern = re.compile(
    "".join([
        fr"    - target: {guid_pattern}\r?\n",
        r"      propertyPath: (serializedProgramAsset|serializationData\..*)\r?\n",
        r"      value:.*\r?\n",
        fr"      objectReference: {guid_pattern}\r?\n",
        r"|",
        fr"  serialized(Udon)?ProgramAsset: {guid_pattern}\r?\n",
        r"|",
        r"    SerializedFormat: [02]\r?\n",
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
