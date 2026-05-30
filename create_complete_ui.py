"""
Crea el menú principal + panel de ajustes completos
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
    """Crea el menú principal"""

    # Cargar fondo
    bg = Image.open(FONDO).convert("RGBA")
    target_width, target_height = 1920, 1080
    bg = bg.resize((target_width, target_height), Image.LANCZOS)

    canvas = Image.new("RGBA", (target_width, target_height))
    canvas.paste(bg, (0, 0))

    print("🎨 MENÚ PRINCIPAL")
    print(f"✓ Fondo: {target_width}x{target_height}")

    # Panel título
    title_panel = load_sprite("Main_tiles", 63)  # 135x60px
    if title_panel:
        scale = 5
        title_panel = title_panel.resize((title_panel.width * scale, title_panel.height * scale), Image.NEAREST)
        x = (target_width - title_panel.width) // 2
        y = 120
        canvas.paste(title_panel, (x, y), title_panel)
        print(f"✓ Panel título: en ({x}, {y})")

    # Botones (usando panel Settings_0 pero más pequeño)
    button_panel = load_sprite("Settings", 0)  # 100x150px
    if button_panel:
        scale = 2.5
        btn_w = int(button_panel.width * scale)
        btn_h = int(button_panel.height * scale)

        buttons_y_start = 450
        button_spacing = 120

        for i, label in enumerate(["JUGAR", "AJUSTES", "SALIR"]):
            btn_scaled = button_panel.resize((btn_w, btn_h), Image.NEAREST)
            x = (target_width - btn_w) // 2
            y = buttons_y_start + (i * button_spacing)
            canvas.paste(btn_scaled, (x, y), btn_scaled)
            print(f"✓ Botón '{label}': en ({x}, {y})")

    canvas.save("menu_main.png")
    print(f"✅ menu_main.png guardado\n")
    return canvas

def create_settings_panel():
    """Crea el panel de ajustes"""

    target_width, target_height = 1920, 1080
    canvas = Image.new("RGBA", (target_width, target_height), (0, 0, 0, 200))

    print("⚙️ PANEL DE AJUSTES")

    # Panel grande central
    big_panel = load_sprite("Settings", 0)  # 100x150px
    if big_panel:
        scale = 6
        panel_w = int(big_panel.width * scale)
        panel_h = int(big_panel.height * scale)
        panel_scaled = big_panel.resize((panel_w, panel_h), Image.NEAREST)

        x = (target_width - panel_w) // 2
        y = (target_height - panel_h) // 2
        canvas.paste(panel_scaled, (x, y), panel_scaled)
        print(f"✓ Panel principal: {panel_w}x{panel_h} en ({x}, {y})")

        # Título "AJUSTES"
        title_panel = load_sprite("Main_tiles", 63)
        if title_panel:
            title_scaled = title_panel.resize((title_panel.width * 3, title_panel.height * 3), Image.NEAREST)
            tx = x + (panel_w - title_scaled.width) // 2
            ty = y + 40
            canvas.paste(title_scaled, (tx, ty), title_scaled)
            print(f"✓ Título: en ({tx}, {ty})")

        # Botones de modo de pantalla (3 botones horizontales)
        btn_panel = load_sprite("Main_tiles", 63)
        if btn_panel:
            btn_scale = 2
            btn_w = btn_panel.width * btn_scale
            btn_h = btn_panel.height * btn_scale

            btn_y = y + 200
            btn_spacing = 150
            btn_start_x = x + (panel_w - (btn_w * 3 + btn_spacing * 2)) // 2

            for i, label in enumerate(["VENTANA", "SIN BORDE", "COMPLETA"]):
                btn_scaled = btn_panel.resize((btn_w, btn_h), Image.NEAREST)
                bx = btn_start_x + i * (btn_w + btn_spacing)
                canvas.paste(btn_scaled, (bx, btn_y), btn_scaled)
                print(f"✓ Botón '{label}': en ({bx}, {btn_y})")

        # Sliders (3 sliders verticales)
        slider_y_start = y + 400
        slider_spacing = 80

        for i, label in enumerate(["GENERAL", "MÚSICA", "EFECTOS"]):
            sy = slider_y_start + i * slider_spacing

            # Barra del slider (rectángulo)
            slider_track = Image.new("RGBA", (400, 20), (80, 60, 40, 255))
            sx = x + (panel_w - 400) // 2
            canvas.paste(slider_track, (sx, sy))

            # Handle del slider
            handle = Image.new("RGBA", (30, 30), (200, 160, 120, 255))
            canvas.paste(handle, (sx + 300, sy - 5))

            print(f"✓ Slider '{label}': en ({sx}, {sy})")

        # Botón VOLVER
        back_btn = load_sprite("Settings", 0)
        if back_btn:
            back_scaled = back_btn.resize((back_btn.width * 2, back_btn.height * 2), Image.NEAREST)
            bx = x + (panel_w - back_scaled.width) // 2
            by = y + panel_h - back_scaled.height - 40
            canvas.paste(back_scaled, (bx, by), back_scaled)
            print(f"✓ Botón VOLVER: en ({bx}, {by})")

    canvas.save("settings_panel.png")
    print(f"✅ settings_panel.png guardado\n")
    return canvas

if __name__ == "__main__":
    create_main_menu()
    create_settings_panel()
    print("🎉 UI completa generada!")
    print("   - menu_main.png: Menú principal")
    print("   - settings_panel.png: Panel de ajustes")
