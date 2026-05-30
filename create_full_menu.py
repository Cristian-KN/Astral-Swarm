"""
Crea el menú principal completo con todos los assets
"""
import sys
import io
sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding='utf-8')

from PIL import Image, ImageDraw, ImageFont
import os

# Rutas
FONDO = r"Assets\Sprites\Downloaded\Imagen fondo\fondo.png"
FRAGMENTOS = r"Assets\Sprites\Downloaded\craftpix-net-255216-free-basic-pixel-art-ui-for-rpg\Fragmentos"
OUTPUT = "menu_final.png"

def load_sprite(folder, index):
    """Carga un sprite"""
    path = os.path.join(FRAGMENTOS, folder, f"{folder}_{index}.png")
    if os.path.exists(path):
        return Image.open(path).convert("RGBA")
    return None

def create_full_menu():
    """Crea el menú completo"""

    # Cargar fondo
    bg = Image.open(FONDO).convert("RGBA")
    width, height = bg.size

    # Escalar fondo a 1920x1080 (resolución estándar)
    target_width, target_height = 1920, 1080
    bg = bg.resize((target_width, target_height), Image.LANCZOS)

    canvas = Image.new("RGBA", (target_width, target_height))
    canvas.paste(bg, (0, 0))

    print(f"✓ Fondo: {target_width}x{target_height}")

    # Cargar panel grande para el título
    title_panel = load_sprite("Main_tiles", 63)  # 135x60px
    if title_panel:
        # Escalar x4
        scale = 5
        title_panel = title_panel.resize((title_panel.width * scale, title_panel.height * scale), Image.NEAREST)
        # Centrar arriba
        x = (target_width - title_panel.width) // 2
        y = 100
        canvas.paste(title_panel, (x, y), title_panel)
        print(f"✓ Panel título: {title_panel.size} en ({x}, {y})")

    # Cargar paneles para botones (usando Settings_0, que es grande)
    button_panel = load_sprite("Settings", 0)  # 100x150px
    if button_panel:
        scale = 3
        button_panel_scaled = button_panel.resize((button_panel.width * scale, button_panel.height * scale), Image.NEAREST)

        # 3 paneles para 3 botones
        buttons_y_start = 400
        button_spacing = button_panel_scaled.height + 40

        for i, label in enumerate(["JUGAR", "AJUSTES", "SALIR"]):
            x = (target_width - button_panel_scaled.width) // 2
            y = buttons_y_start + (i * button_spacing)
            canvas.paste(button_panel_scaled, (x, y), button_panel_scaled)
            print(f"✓ Panel botón '{label}': {button_panel_scaled.size} en ({x}, {y})")

    # Guardar
    canvas.save(OUTPUT)
    print(f"\n✅ Menú guardado: {OUTPUT}")
    print(f"📐 Resolución: {target_width}x{target_height}")

if __name__ == "__main__":
    create_full_menu()
