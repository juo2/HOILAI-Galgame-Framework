import os
import shutil
import subprocess

def copy_images(src_dir, dest_dir):
    """
    复制src_dir目录下的所有图片到dest_dir目录。
    """
    for item in os.listdir(src_dir):
        if item.lower().endswith(('.png', '.jpg', '.jpeg', '.tga', '.gif')):
            src_path = os.path.join(src_dir, item)
            dest_path = os.path.join(dest_dir, item)
            shutil.copy(src_path, dest_path)
            print(f"复制了 {item} 到 {dest_dir}")

def reimport_assets(unity_dir, project_dir):
    """
    使用Unity命令行工具来重新导入项目资源。
    """
    command = [
        unity_dir,
        "-batchmode",
        "-quit",
        "-nographics",
        "-executeMethod", "XBuildCommandLine.BuildReImport",
        "-projectPath", project_dir
    ]
    
    try:
        result = subprocess.run(command, check=True, capture_output=True, text=True)
        print("资源重新导入成功:\n", result.stdout)
    except subprocess.CalledProcessError as e:
        print("资源重新导入失败:\n", e.stderr)

# 设置源目录和目标目录
src_dir = r"C:\Users\mikiliang\Downloads\1111"
dest_dir = r"D:\Git\HOILAI-Galgame-Framework2\Assets\GUI\Modules\wyyglzj\Images\Single"

# 设置Unity编辑器路径和项目路径
unity_dir = r"C:\Program Files\Unity2021.3.32f1\Editor\Unity.exe"
project_dir = r"D:\Git\HOILAI-Galgame-Framework2"

# 复制图片
copy_images(src_dir, dest_dir)

# 重新导入资源
reimport_assets(unity_dir, project_dir)