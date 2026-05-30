"""
Crea UI completa y profesional con todos los sprites bien colocados
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

def create_main_menu():
    """Crea menú principal mejorado"""

    # Fondo
    bg = Image.open(FONDO).convert("RGBA")
    W, H = 1920, 1080
    bg = bg.resize((W, H), Image.LANCZOS)
    canvas = Image.new("RGBA", (W, H))
    canvas.paste(bg, (0, 0))

    print("🎨 MENÚ PRINCIPAL MEJORADO")

    # Panel de título grande (usando Main_tiles_63 que es 135x60)
    title = load_sprite("Main_tiles", 63)
    if title:
        title = title.resize((title.width * 6, title.height * 6), Image.NEAREST)
        x = (W - title.width) // 2
        y = 150
        canvas.paste(title, (x, y), title)
        print(f"✓ Título: {title.size} en ({x}, {y})")

    # Botones grandes (usando Settings_0 panel)
    button_panel = load_sprite("Settings", 0)  # 100x150
    if button_panel:
        scale = 4
        btn = button_panel.resize((button_panel.width * scale, button_panel.height * scale), Image.NEAREST)

        y_start = 480
        spacing = btn.height + 60

        for i, name in enumerate(["JUGAR", "AJUSTES", "SALIR"]):
            x = (W - btn.width) // 2
            y = y_start + i * spacing
            canvas.paste(btn, (x, y), btn)
            print(f"✓ Botón {name}: {btn.size} en ({x}, {y})")

    canvas.save("menu_main_v2.png")
    print(f"✅ menu_main_v2.png guardado\n")

def create_settings_panel():
    """Crea panel de ajustes COMPLETO"""

    W, H = 1920, 1080
    # Fondo oscuro semi-transparente
    canvas = Image.new("RGBA", (W, H), (20, 20, 30, 220))

    print("⚙️ PANEL DE AJUSTES COMPLETO")

    # Panel principal gigante
    main_panel = load_sprite("Settings", 0)  # 100x150
    if not main_panel:
        print("❌ No se pudo cargar panel")
        return

    scale = 7
    panel = main_panel.resize((main_panel.width * scale, main_panel.height * scale), Image.NEAREST)
    px = (W - panel.width) // 2
    py = (H - panel.height) // 2
    canvas.paste(panel, (px, py), panel)
    print(f"✓ Panel principal: {panel.size} en ({px}, {py})")

    # TÍTULO "AJUSTES"
    title_panel = load_sprite("Main_tiles", 63)  # 135x60
    if title_panel:
        title = title_panel.resize((title_panel.width * 4, title_panel.height * 4), Image.NEAREST)
        tx = px + (panel.width - title.width) // 2
        ty = py + 60
        canvas.paste(title, (tx, ty), title)
        print(f"✓ Título: en ({tx}, {ty})")

    # BOTONES DE MODO DE PANTALLA (3 botones horizontales)
    mode_btn = load_sprite("Settings", 0)  # Usar mismo panel pequeño
    if mode_btn:
        btn_scale = 2
        btn = mode_btn.resize((mode_btn.width * btn_scale, mode_btn.height * btn_scale), Image.NEAREST)

        btn_y = py + 300
        total_width = (btn.width * 3) + (100 * 2)  # 3 botones + 2 espacios
        start_x = px + (panel.width - total_width) // 2

        for i, label in enumerate(["VENTANA", "SIN BORDE", "COMPLETA"]):
            bx = start_x + i * (btn.width + 100)
            canvas.paste(btn, (bx, btn_y), btn)
            print(f"✓ Botón modo '{label}': en ({bx}, {btn_y})")

    # SLIDERS (usando sprites de barra)
    slider_y_start = py + 550
    slider_spacing = 100

    # Barra base del slider (crear con sprites)
    bar_segment = load_sprite("Main_tiles", 65)  # 26x14 - segmento de barra
    if bar_segment:
        bar_segment = bar_segment.resize((bar_segment.width * 2, bar_segment.height * 3), Image.NEAREST)

        for i, label in enumerate(["VOL. GENERAL", "MÚSICA", "EFECTOS"]):
            sy = slider_y_start + i * slider_spacing

            # Barra (repetir segmento)
            bar_width = 450
            bar_x = px + (panel.width - bar_width) // 2

            # Fondo de barra
            for bx in range(bar_x, bar_x + bar_width, bar_segment.width):
                canvas.paste(bar_segment, (bx, sy), bar_segment)

            # Handle del slider (botoncito)
            handle = load_sprite("Buttons", 20)  # 44x34 - botón cuadrado
            if handle:
                handle = handle.resize((handle.width * 2, handle.height * 2), Image.NEAREST)
                handle_x = bar_x + 300  # 75% del slider
                canvas.paste(handle, (handle_x, sy - 10), handle)

            print(f"✓ Slider '{label}': en ({bar_x}, {sy})")

    # BOTÓN VOLVER
    back_btn = load_sprite("Settings", 0)
    if back_btn:
        back_btn = back_btn.resize((back_btn.width * 3, back_btn.height * 3), Image.NEAREST)
        bx = px + (panel.width - back_btn.width) // 2
        by = py + panel.height - back_btn.height - 80
        canvas.paste(back_btn, (bx, by), back_btn)
        print(f"✓ Botón VOLVER: en ({bx}, {by})")

    canvas.save("settings_panel_v2.png")
    print(f"✅ settings_panel_v2.png guardado\n")

if __name__ == "__main__":
    create_main_menu()
    create_settings_panel()
    print("🎉 UI v2 completa!")
