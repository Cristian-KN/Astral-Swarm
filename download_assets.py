import os
import requests
from bs4 import BeautifulSoup
from urllib.parse import urljoin

def download_assets():
    """
    Descarga automáticamente assets CC0/CC-BY para el proyecto Astral Swarm.
    """
    target_dir = os.path.join(os.getcwd(), "Assets", "Sprites", "Downloads")
    if not os.path.exists(target_dir):
        os.makedirs(target_dir)
        print(f"[*] Directorio creado: {target_dir}")

    urls = [
        "https://opengameart.org/content/lpc-monsters",                         # Slimes, murciélagos (Normal/Rápido)
        "https://opengameart.org/content/lpc-medieval-fantasy-character-sprites", # Héroes (Caballero/Mago)
        "https://opengameart.org/content/50-monsters-pack-2d",                  # Golems y Bosses (Tanque/Jefe)
        "https://opengameart.org/content/top-down-game-assets",                 # Armas y objetos
        "https://opengameart.org/content/projectile-attacks-art-collection",    # Balas y efectos mágicos
        "https://opengameart.org/content/tiny-pixel-art-icons-pack"             # Iconos para la tienda
    ]

    headers = {
        "User-Agent": "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36"
    }

    print("--- Iniciando descarga de Assets para Astral Swarm ---")

    for url in urls:
        try:
            print(f"\n[>] Analizando: {url}")
            response = requests.get(url, headers=headers)
            response.raise_for_status()
            
            soup = BeautifulSoup(response.text, 'html.parser')
            
            files_found = 0
            for link in soup.find_all('a'):
                href = link.get('href')
                # Filtramos links de descarga reales de OpenGameArt
                if href and "/sites/default/files/" in href:
                    file_url = urljoin(url, href)
                    file_name = href.split('/')[-1]
                    
                    # Solo descargamos formatos útiles para Unity
                    if any(file_name.lower().endswith(ext) for ext in ['.png', '.zip', '.jpg']):
                        download_file(file_url, file_name, target_dir, headers)
                        files_found += 1
            
            if files_found == 0:
                print(f"[!] No se encontraron archivos directos en {url}. Puede que requieran descarga manual.")

        except Exception as e:
            print(f"[X] Error procesando {url}: {e}")

    print("\n--- ¡Descarga Completada! ---")
    print(f"Encuentra tus assets en: {target_dir}")
    print("IMPORTANTE: Descomprime los archivos .zip y configura los Sprites en Unity como 'Filter Mode: Point'.")

def download_file(url, file_name, folder, headers):
    path = os.path.join(folder, file_name)
    
    if os.path.exists(path):
        print(f"[-] Omitiendo {file_name} (Ya existe)")
        return

    try:
        with requests.get(url, headers=headers, stream=True) as r:
            r.raise_for_status()
            with open(path, 'wb') as f:
                for chunk in r.iter_content(chunk_size=8192):
                    f.write(chunk)
        print(f"[+] Descargado: {file_name}")
    except Exception as e:
        print(f"[X] Error descargando {file_name}: {e}")

if __name__ == "__main__":
    try:
        download_assets()
    except ImportError:
        print("Error: Por favor instala las dependencias con 'pip install requests beautifulsoup4'")
    except KeyboardInterrupt:
        print("\nDescarga cancelada por el usuario.")