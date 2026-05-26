"""
Generador de Previews de Biomas para Astral Swarm
Genera imágenes PNG de todos los biomas configurados para preview visual
"""

import sys
import io
sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding='utf-8')

import numpy as np
from PIL import Image, ImageDraw, ImageFont
import random
import os

# Crear carpeta de output
OUTPUT_DIR = "biome_previews"
os.makedirs(OUTPUT_DIR, exist_ok=True)

def perlin_noise_2d(shape, scale=10, seed=None):
    """Genera Perlin noise 2D simplificado usando interpolación"""
    if seed is not None:
        np.random.seed(seed)

    h, w = shape
    # Crear grid de valores aleatorios
    grid_h = max(2, h // scale)
    grid_w = max(2, w // scale)
    grid = np.random.rand(grid_h, grid_w)

    # Interpolar para hacer smooth
    from scipy.ndimage import zoom
    noise = zoom(grid, (h / grid_h, w / grid_w), order=1)

    # Normalizar a 0-1
    noise = (noise - noise.min()) / (noise.max() - noise.min())
    return noise

def color_to_rgb(r, g, b):
    """Convierte valores float 0-1 a RGB 0-255"""
    return (int(r * 255), int(g * 255), int(b * 255))

def add_stars(image, density=0.02, size=1, seed=None):
    """Añade estrellas al fondo"""
    if seed is not None:
        random.seed(seed)

    width, height = image.size
    pixels = image.load()

    star_count = int(width * height * density)

    for _ in range(star_count):
        x = random.randint(0, width - 1)
        y = random.randint(0, height - 1)
        brightness = random.uniform(0.6, 1.0)
        star_color = (int(255 * brightness), int(255 * brightness), int(255 * brightness))

        if size == 1:
            pixels[x, y] = star_color
        else:
            for dx in range(size):
                for dy in range(size):
                    px, py = x + dx, y + dy
                    if 0 <= px < width and 0 <= py < height:
                        pixels[px, py] = star_color

def generate_deep_space(width=512, height=512, primary=(13, 13, 31), secondary=(20, 20, 46),
                        accent=(51, 51, 102), star_density=0.015, nebula_density=0.2, seed=42):
    """Genera fondo de Deep Space"""
    image = Image.new('RGB', (width, height))
    pixels = image.load()

    # Generar Perlin noise para variación
    noise = perlin_noise_2d((height, width), scale=20, seed=seed)

    for y in range(height):
        for x in range(width):
            n = noise[y, x] * nebula_density
            r = int(primary[0] * (1 - n) + secondary[0] * n)
            g = int(primary[1] * (1 - n) + secondary[1] * n)
            b = int(primary[2] * (1 - n) + secondary[2] * n)
            pixels[x, y] = (r, g, b)

    add_stars(image, star_density, size=1, seed=seed)
    return image

def generate_nebula(width=512, height=512, primary=(13, 13, 31), secondary=(20, 20, 46),
                   accent=(51, 51, 102), star_density=0.02, nebula_density=0.5, seed=42):
    """Genera fondo de nebulosa con múltiples octavas de Perlin noise"""
    image = Image.new('RGB', (width, height))
    pixels = image.load()

    # Múltiples octavas de noise
    noise1 = perlin_noise_2d((height, width), scale=50, seed=seed)
    noise2 = perlin_noise_2d((height, width), scale=20, seed=seed * 2) * 0.5
    noise3 = perlin_noise_2d((height, width), scale=10, seed=seed * 3) * 0.25

    combined = (noise1 + noise2 + noise3) / 1.75
    combined = np.power(combined, 2)  # Aumentar contraste

    for y in range(height):
        for x in range(width):
            n = combined[y, x] * nebula_density
            r = int(primary[0] * (1 - n) + accent[0] * n)
            g = int(primary[1] * (1 - n) + accent[1] * n)
            b = int(primary[2] * (1 - n) + accent[2] * n)
            pixels[x, y] = (r, g, b)

    add_stars(image, star_density * 0.5, size=1, seed=seed)
    return image

def generate_starfield(width=512, height=512, primary=(5, 5, 13), star_density=0.06, seed=42):
    """Genera campo denso de estrellas"""
    image = Image.new('RGB', (width, height), color=primary)

    add_stars(image, star_density * 3, size=1, seed=seed)
    add_stars(image, star_density * 2, size=2, seed=seed + 100)

    return image

def generate_binary_stars(width=512, height=512, primary=(13, 20, 38), accent=(204, 128, 77),
                         star_density=0.02, seed=42):
    """Genera sistema estelar binario con resplandor"""
    image = Image.new('RGB', (width, height))
    pixels = image.load()

    # Posiciones de las estrellas
    star1 = np.array([width * 0.3, height * 0.6])
    star2 = np.array([width * 0.7, height * 0.4])

    for y in range(height):
        for x in range(width):
            pos = np.array([x, y])

            dist1 = np.linalg.norm(pos - star1) / width
            dist2 = np.linalg.norm(pos - star2) / width

            glow1 = max(0, 1 - dist1 * 2) * 0.3
            glow2 = max(0, 1 - dist2 * 2) * 0.2

            glow_total = glow1 + glow2
            r = int(primary[0] * (1 - glow_total) + accent[0] * glow_total)
            g = int(primary[1] * (1 - glow_total) + accent[1] * glow_total)
            b = int(primary[2] * (1 - glow_total) + accent[2] * glow_total)

            pixels[x, y] = (r, g, b)

    add_stars(image, star_density, size=1, seed=seed)
    return image

def add_label(image, text, color=(255, 255, 255)):
    """Añade etiqueta de texto a la imagen"""
    draw = ImageDraw.Draw(image)

    # Intentar usar fuente grande, si no existe usar default
    try:
        font = ImageFont.truetype("arial.ttf", 32)
    except:
        font = ImageFont.load_default()

    # Calcular posición centrada
    bbox = draw.textbbox((0, 0), text, font=font)
    text_width = bbox[2] - bbox[0]
    text_height = bbox[3] - bbox[1]

    x = (image.width - text_width) // 2
    y = 20

    # Fondo semi-transparente para legibilidad
    padding = 10
    draw.rectangle([x - padding, y - padding, x + text_width + padding, y + text_height + padding],
                   fill=(0, 0, 0, 200))

    draw.text((x, y), text, fill=color, font=font)

# Definición de todos los biomas
BIOMES = [
    # Biomas Normales
    {
        "name": "01_VoidSpace",
        "display_name": "Vacío Espacial",
        "type": "nebula",
        "primary": (13, 13, 31),
        "secondary": (20, 20, 46),
        "accent": (51, 51, 102),
        "star_density": 0.015,
        "nebula_density": 0.2,
        "seed": 42069
    },
    {
        "name": "02_CrimsonNebula",
        "display_name": "Nebulosa Carmesí",
        "type": "nebula",
        "primary": (38, 5, 13),
        "secondary": (64, 13, 26),
        "accent": (204, 51, 77),
        "star_density": 0.02,
        "nebula_density": 0.5,
        "seed": 12345
    },
    {
        "name": "03_FrozenVoid",
        "display_name": "Vacío Helado",
        "type": "nebula",
        "primary": (5, 20, 38),
        "secondary": (13, 31, 64),
        "accent": (77, 153, 230),
        "star_density": 0.025,
        "nebula_density": 0.3,
        "seed": 67890
    },
    {
        "name": "04_ToxicCloud",
        "display_name": "Nube Tóxica",
        "type": "nebula",
        "primary": (13, 31, 5),
        "secondary": (20, 46, 13),
        "accent": (102, 230, 77),
        "star_density": 0.01,
        "nebula_density": 0.6,
        "seed": 11111
    },
    {
        "name": "05_ElectricStorm",
        "display_name": "Tormenta Eléctrica",
        "type": "nebula",
        "primary": (31, 31, 5),
        "secondary": (51, 51, 13),
        "accent": (230, 230, 77),
        "star_density": 0.03,
        "nebula_density": 0.4,
        "seed": 22222
    },
    {
        "name": "06_DeepAbyss",
        "display_name": "Abismo Profundo",
        "type": "nebula",
        "primary": (20, 5, 31),
        "secondary": (31, 13, 51),
        "accent": (153, 77, 230),
        "star_density": 0.018,
        "nebula_density": 0.5,
        "seed": 33333
    },
    {
        "name": "07_SolarFlare",
        "display_name": "Llamarada Solar",
        "type": "nebula",
        "primary": (46, 20, 5),
        "secondary": (77, 38, 13),
        "accent": (230, 128, 51),
        "star_density": 0.02,
        "nebula_density": 0.4,
        "seed": 44444
    },
    {
        "name": "08_CosmicRift",
        "display_name": "Grieta Cósmica",
        "type": "nebula",
        "primary": (26, 13, 38),
        "secondary": (38, 20, 64),
        "accent": (204, 102, 230),
        "star_density": 0.035,
        "nebula_density": 0.6,
        "seed": 55555
    },
    {
        "name": "09_DarkMatter",
        "display_name": "Materia Oscura",
        "type": "deep_space",
        "primary": (5, 5, 5),
        "secondary": (13, 13, 20),
        "accent": (77, 77, 128),
        "star_density": 0.008,
        "nebula_density": 0.2,
        "seed": 66666
    },
    {
        "name": "10_VoidEdge",
        "display_name": "Borde del Vacío",
        "type": "nebula",
        "primary": (38, 20, 46),
        "secondary": (64, 38, 77),
        "accent": (204, 128, 230),
        "star_density": 0.04,
        "nebula_density": 0.7,
        "seed": 77777
    },

    # Bioma Especial
    {
        "name": "11_GoldenAnomaly_SPECIAL",
        "display_name": "⭐ ANOMALÍA DORADA ⭐",
        "type": "nebula",
        "primary": (38, 31, 5),
        "secondary": (64, 51, 13),
        "accent": (242, 204, 77),
        "star_density": 0.05,
        "nebula_density": 0.4,
        "seed": 99999
    }
]

def generate_biome_preview(biome_config, width=512, height=512):
    """Genera preview de un bioma basado en su configuración"""
    biome_type = biome_config["type"]

    if biome_type == "deep_space":
        image = generate_deep_space(
            width, height,
            biome_config["primary"],
            biome_config["secondary"],
            biome_config["accent"],
            biome_config["star_density"],
            biome_config["nebula_density"],
            biome_config["seed"]
        )
    elif biome_type == "nebula":
        image = generate_nebula(
            width, height,
            biome_config["primary"],
            biome_config["secondary"],
            biome_config["accent"],
            biome_config["star_density"],
            biome_config["nebula_density"],
            biome_config["seed"]
        )
    elif biome_type == "starfield":
        image = generate_starfield(
            width, height,
            biome_config["primary"],
            biome_config["star_density"],
            biome_config["seed"]
        )
    elif biome_type == "binary":
        image = generate_binary_stars(
            width, height,
            biome_config["primary"],
            biome_config["accent"],
            biome_config["star_density"],
            biome_config["seed"]
        )
    else:
        image = generate_deep_space(width, height, seed=biome_config["seed"])

    # Añadir etiqueta
    label_color = (242, 204, 77) if "SPECIAL" in biome_config["name"] else (255, 255, 255)
    add_label(image, biome_config["display_name"], label_color)

    return image

def generate_all_previews():
    """Genera todas las previews de biomas"""
    print(f"Generando {len(BIOMES)} previews de biomas...")

    for biome in BIOMES:
        print(f"  → Generando {biome['display_name']}...")

        image = generate_biome_preview(biome, width=512, height=512)

        filename = f"{OUTPUT_DIR}/{biome['name']}.png"
        image.save(filename)
        print(f"    ✓ Guardado: {filename}")

    print(f"\n✅ Todas las previews generadas en '{OUTPUT_DIR}/'")

    # Generar composición de todas juntas
    print("\nGenerando composición de galería...")
    generate_gallery()

def generate_gallery():
    """Genera una galería con todos los biomas juntos"""
    cols = 4
    rows = 3
    thumb_size = 256
    margin = 10

    gallery_width = cols * thumb_size + (cols + 1) * margin
    gallery_height = rows * thumb_size + (rows + 1) * margin

    gallery = Image.new('RGB', (gallery_width, gallery_height), color=(10, 10, 20))

    for idx, biome in enumerate(BIOMES):
        row = idx // cols
        col = idx % cols

        x = margin + col * (thumb_size + margin)
        y = margin + row * (thumb_size + margin)

        # Generar thumbnail
        thumb = generate_biome_preview(biome, width=thumb_size, height=thumb_size)
        gallery.paste(thumb, (x, y))

    gallery_path = f"{OUTPUT_DIR}/00_GALLERY.png"
    gallery.save(gallery_path)
    print(f"✓ Galería guardada: {gallery_path}")

if __name__ == "__main__":
    try:
        # Verificar dependencias
        print("Verificando dependencias...")
        try:
            from scipy.ndimage import zoom
            print("✓ scipy encontrado")
        except ImportError:
            print("⚠️ scipy no encontrado, instalando...")
            os.system("pip install scipy")

        generate_all_previews()

    except Exception as e:
        print(f"\n❌ Error: {e}")
        import traceback
        traceback.print_exc()
