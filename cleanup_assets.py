import os
import shutil

def cleanup_old_assets():
    """
    Elimina la carpeta de descargas para limpiar los assets antiguos que ya no se usan.
    """
    target_dir = os.path.join(os.getcwd(), "Assets", "Sprites", "Downloads")
    
    if os.path.exists(target_dir):
        print(f"[*] Limpiando carpeta de assets: {target_dir}")
        try:
            shutil.rmtree(target_dir)
            print("[+] Limpieza completada. Los assets antiguos han sido borrados.")
            print("[!] Ahora puedes ejecutar 'python download_assets.py' para bajar solo los mejores.")
        except Exception as e:
            print(f"[X] Error al limpiar la carpeta: {e}")
    else:
        print("[-] No se encontró la carpeta de descargas. Ya está limpia.")

if __name__ == "__main__":
    confirm = input("¿Confirmas que quieres borrar TODOS los assets descargados anteriormente? (s/n): ")
    if confirm.lower() == 's':
        cleanup_old_assets()