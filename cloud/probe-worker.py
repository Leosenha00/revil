"""Reads capabilities and creates one result.json; no quota or eligibility claim."""
import json, os, platform, shutil
from pathlib import Path
memory = None
if Path('/proc/meminfo').exists():
    for line in Path('/proc/meminfo').read_text().splitlines():
        if line.startswith('MemTotal:'):
            memory = int(line.split()[1]) * 1024
result = dict(os=platform.system(), architecture=platform.machine(), cpu_logical=os.cpu_count(),
    memory_bytes=memory, tools={name: shutil.which(name) for name in ['python3','ffmpeg','blender','sha256sum']},
    disk_free_bytes=shutil.disk_usage('.').free, quota_verified=False, autocad_windows_supported=False)
text = json.dumps(result, indent=2)
print(text)
with open("result.json", "x", encoding="utf-8") as output:
    output.write(text)

