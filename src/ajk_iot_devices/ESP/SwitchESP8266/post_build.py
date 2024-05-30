import shutil
import os

from SCons.Script import DefaultEnvironment

env = DefaultEnvironment()

def after_build(source, target, env):
    # Ścieżka do wynikowego pliku binarnego
    firmware_path = os.path.join(env.subst("$BUILD_DIR"), "firmware.bin")
    
    # Docelowa ścieżka, gdzie plik ma być skopiowany
    # Przykład: skopiowanie do trzech poziomów wyżej w folderze output
    project_dir = env.subst("$PROJECT_DIR")
    destination_dir = os.path.abspath(os.path.join(project_dir, '..', '..', '..', 'AJKIOT.Api', 'FirmwareFiles', 'SwitchEsp12E'))
    destination_path = os.path.join(destination_dir, "FirmwareSwitch.bin")
    
    # Utwórz folder docelowy, jeśli nie istnieje
    os.makedirs(os.path.dirname(destination_path), exist_ok=True)
    
    # Skopiuj plik
    shutil.copy(firmware_path, destination_path)
    print(f"Firmware copied to {destination_path}")

# Dodaj akcję post-build
env.AddPostAction("buildprog", after_build)
