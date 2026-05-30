"""
Explora todos los sprites para ver qué tenemos disponible
"""
import sys
import io
sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding='utf-8')

from PIL import Image
import os

FRAGMENTOS = r"Assets\Sprites\Downloaded\craftpix-net-255216-free-basic-pixel-art-ui-for-rpg\Fragmentos"

def explore_folder(folder_name):
    """Explora una carpeta de sprites"""
    folder_path = os.path.join(FRAGMENTOS, folder_name)
    if not os.path.exists(folder_path):
        return

    print(f"\n📁 {folder_name}:")
    sprites = {}

    for file in os.listdir(folder_path):
        if file.endswith('.png') and not file.endswith('.meta'):
            path = os.path.join(folder_path, file)
            try:
                img = Image.open(path)
                sprites[file] = img.size
            except:
                pass

    # Agrupar por tamaño
    sizes = {}
    for name, size in sprites.items():
        if size not in sizes:
            sizes[size] = []
        sizes[size].append(name)

    # Mostrar agrupados
    for size in sorted(sizes.keys(), key=lambda x: x[0] * x[1], reverse=True):
        files = sizes[size]
        print(f"   {size[0]}x{size[1]}px ({len(files)} sprites):")
        for f in files[:5]:  # Mostrar solo los primeros 5
            print(f"      - {f}")
        if len(files) > 5:
            print(f"      ... y {len(files) - 5} más")

# Explorar carpetas principales
folders = ["Main_menu", "Buttons", "Settings", "Main_tiles", "Text1", "Text2"]

for folder in folders:
    explore_folder(folder)

print("\n✅ Exploración completa")
