# 📋 Arquetipos de Enemigos - Astral Swarm

Estos son los 5 moldes base. Cualquier enemigo en el juego pertenece a uno de estos:

| Arquetipo | Asset Sugerido | HP Base | Vel. Base | Daño Base | Comportamiento |
| :--- | :--- | :---: | :---: | :---: | :--- |
| **Normal** | Slime / Esqueleto | 100 | 3.0 | 10 | Camina hacia el jugador. |
| **Rápido** | Murciélago | 50 | 5.5 | 5 | Muy veloz, poca vida. |
| **Tirador** | Ojo Flotante | 80 | 2.0 | 15 | Dispara proyectiles a distancia. |
| **Tanque** | Gólem / Zombie | 400 | 1.5 | 25 | Lento, difícil de empujar. |
| **Jefe** | Demonio Grande | 2000 | 2.5 | 50 | Gran tamaño, hitbox amplia. |

### Notas de Escalado
*   La **Escala Visual** (`transform.scale`) debe variar: Rápido (0.8x), Normal (1x), Tanque (1.6x), Jefe (3x).
*   Los stats finales se calculan multiplicando estos valores base por el multiplicador de la **Variante** (definida en `EnemyVariants.md`).