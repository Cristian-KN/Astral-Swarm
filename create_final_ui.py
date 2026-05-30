"""
Crea UI final con espaciado correcto y texto
"""
import sys
import io
sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding='utf-8')

from PIL import Image, ImageDraw, ImageFont
import os

# Rutas
FONDO = r"Assets\Sprites\Downloaded\Imagen fondo\fondo.png"
FRAGMENTOS = r"Assets\Sprites\Downloaded\craftpix-net-255216-free-basic-pixel-art-ui-for-rpg\Fragmentos"

def load_sprite(folder, index):
    """Carga un sprite"""
    path = os.path.join(FRAGMENTOS, folder, f"{folder}_{index}.png")
    if os.path.exists(path):
        return Image.open(path).convert("RGBA")
    return None

def add_text(canvas, text, x, y, size=40, color=(255, 255, 255)):
    """Añade texto pixelado al canvas"""
    draw = ImageDraw.Draw(canvas)
    # Usar fuente por defecto si no hay otra
    try:
        font = ImageFont.truetype("arial.ttf", size)
    except:
        font = ImageFont.load_default()
    draw.text((x, y), text, fill=color, font=font)

def create_main_menu():
    """Menú principal con espaciado correcto"""

    bg = Image.open(FONDO).convert("RGBA")
    W, H = 1920, 1080
    bg = bg.resize((W, H), Image.LANCZOS)
    canvas = Image.new("RGBA", (W, H))
    canvas.paste(bg, (0, 0))

    print("🎨 MENÚ PRINCIPAL FINAL")

    # Título
    title = load_sprite("Main_tiles", 63)
    if title:
        title = title.resize((title.width * 6, title.height * 6), Image.NEAREST)
        x = (W - title.width) // 2
        y = 180
        canvas.paste(title, (x, y), title)
        add_text(canvas, "ASTRAL SWARM", x + 80, y + 120, size=60, color=(255, 230, 150))
        print(f"✓ Título en ({x}, {y})")

    # Botones con espaciado CORRECTO
    button_panel = load_sprite("Settings", 0)
    if button_panel:
        scale = 3.5
        btn = button_panel.resize((int(button_panel.width * scale), int(button_panel.height * scale)), Image.NEAREST)

        y_start = 500
        spacing = 140  # Espaciado reducido para que quepan los 3

        for i, (name, color) in enumerate([("JUGAR", (100, 255, 100)),
                                            ("AJUSTES", (150, 150, 255)),
                                            ("SALIR", (255, 100, 100))]):
            x = (W - btn.width) // 2
            y = y_start + i * spacing
            canvas.paste(btn, (x, y), btn)

            # Texto centrado en el botón
            text_x = x + btn.width // 2 - len(name) * 15
            text_y = y + btn.height // 2 - 20
            add_text(canvas, name, text_x, text_y, size=48, color=color)
            print(f"✓ Botón {name} en ({x}, {y})")

    canvas.save("MENU_FINAL.png")
    print(f"✅ MENU_FINAL.png guardado\n")

def create_settings_panel():
    """Panel de ajustes con labels"""

    W, H = 1920, 1080
    canvas = Image.new("RGBA", (W, H), (20, 20, 30, 230))

    print("⚙️ PANEL AJUSTES FINAL")

    # Panel principal
    main_panel = load_sprite("Settings", 0)
    if not main_panel:
        return

    scale = 7
    panel = main_panel.resize((main_panel.width * scale, main_panel.height * scale), Image.NEAREST)
    px = (W - panel.width) // 2
    py = 50
    canvas.paste(panel, (px, py), panel)
    print(f"✓ Panel en ({px}, {py})")

    # Título
    title = load_sprite("Main_tiles", 63)
    if title:
        title = title.resize((title.width * 4, title.height * 4), Image.NEAREST)
        tx = px + (panel.width - title.width) // 2
        ty = py + 80
        canvas.paste(title, (tx, ty), title)
        add_text(canvas, "AJUSTES", tx + 140, ty + 80, size=55, color=(255, 230, 150))

    # Botones de modo
    mode_btn = load_sprite("Settings", 0)
    if mode_btn:
        btn = mode_btn.resize((mode_btn.width * 2, mode_btn.height * 2), Image.NEAREST)
        btn_y = py + 350
        spacing = 120

        for i, (label, color) in enumerate([("VENTANA", (150, 200, 150)),
                                             ("SIN BORDE", (200, 200, 100)),
                                             ("COMPLETA", (100, 200, 200))]):
            bx = px + 120 + i * (btn.width + spacing)
            canvas.paste(btn, (bx, btn_y), btn)
            add_text(canvas, label, bx + 20, btn_y + 130, size=28, color=color)
            print(f"✓ Modo '{label}' en ({bx}, {btn_y})")

    # Sliders con LABELS
    slider_y_start = py + 580
    slider_spacing = 100

    bar_segment = load_sprite("Main_tiles", 65)
    if bar_segment:
        bar_segment = bar_segment.resize((bar_segment.width * 2, bar_segment.height * 3), Image.NEAREST)

        for i, (label, color) in enumerate([("VOLUMEN GENERAL", (200, 200, 200)),
                                             ("MÚSICA", (150, 200, 255)),
                                             ("EFECTOS", (255, 200, 150))]):
            sy = slider_y_start + i * slider_spacing

            # Label a la izquierda
            add_text(canvas, label, px + 80, sy, size=32, color=color)

            # Barra
            bar_width = 400
            bar_x = px + 320

            for bx in range(bar_x, bar_x + bar_width, bar_segment.width):
                canvas.paste(bar_segment, (bx, sy), bar_segment)

            # Handle
            handle = load_sprite("Buttons", 20)
            if handle:
                handle = handle.resize((handle.width * 2, handle.height * 2), Image.NEAREST)
                handle_x = bar_x + 250
                canvas.paste(handle, (handle_x, sy - 10), handle)

            print(f"✓ Slider '{label}' en ({bar_x}, {sy})")

    # Botón VOLVER
    back_btn = load_sprite("Settings", 0)
    if back_btn:
        back_btn = back_btn.resize((back_btn.width * 2, back_btn.height * 2), Image.NEAREST)
        bx = px + (panel.width - back_btn.width) // 2
        by = py + panel.height - 200
        canvas.paste(back_btn, (bx, by), back_btn)
        add_text(canvas, "VOLVER", bx + 60, by + 120, size=40, color=(200, 200, 100))
        print(f"✓ Botón VOLVER en ({bx}, {by})")

    canvas.save("SETTINGS_FINAL.png")
    print(f"✅ SETTINGS_FINAL.png guardado\n")

if __name__ == "__main__":
    create_main_menu()
    create_settings_panel()
    print("🎉 UI FINAL COMPLETA!")
    print("   - MENU_FINAL.png")
    print("   - SETTINGS_FINAL.png")
