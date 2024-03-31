import subprocess

# 设置变量
unity_dir = r"C:\Program Files\Unity2021.3.32f1\Editor\Unity.exe"
project_dir = r"C:\ProgramData\Jenkins\.jenkins\workspace\unityweb"
platform = "WebGL"

# 构造命令
command = [
    unity_dir,
    "-batchmode",
    "-quit",
    "-nographics",
    "-executeMethod", "XBuildCommandLine.BuildDevProject",
    "-projectPath", project_dir,
    "-platform", platform
]

# 执行命令
try:
    result = subprocess.run(command, check=True, capture_output=True, text=True)
    print("命令执行成功:\n", result.stdout)
except subprocess.CalledProcessError as e:
    print("命令执行失败:\n", e.stderr)