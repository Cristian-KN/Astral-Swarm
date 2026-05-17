# 🛡️ Personajes y Equipamiento - Astral Swarm

Este documento define las clases de personajes, los límites de inventario y el sistema de mejora al subir de nivel.

## 🎭 Personajes Iniciales

Cada personaje empieza con un arma base fija que ocupa el primer slot de armas.

| Personaje | Arma Inicial | Estilo de Juego |
| :--- | :--- | :--- |
| **Caballero** | Espada | Cuerpo a cuerpo / Supervivencia |
| **Mago** | Bastón (Misil Mágico) | Distancia / Daño Progresivo |
| **Berserker** (Bloqueado) | Daño en Área | Agresivo / Control de Masas |

---

## 🎒 Sistema de Slots (Inventario)

Para evitar que el jugador sea invencible demasiado rápido, existen límites en los tipos de equipo activo:

1.  **Armas (Max 3)**:
    *   Slot 1: Arma base del personaje (Fija).
    *   Slot 2 y 3: Armas adicionales que se encuentran en la partida.
2.  **Habilidades Activas (Max 3)**:
    *   Objetos que disparan o actúan con un retraso (ej: Rayo cada 3s, Nova de fuego cada 5s).
3.  **Pasivas (Infinitas)**:
    *   Mejoras de estadísticas, objetos evolutivos, multiplicadores, etc. No ocupan slots limitados.

---

## 🔝 Subida de Nivel y Mejoras

Al subir de nivel, se presentan varias opciones aleatorias:

### 1. Adquisición de Equipo
Si el jugador tiene slots libres, puede elegir un arma o habilidad activa nueva.

### 2. Mejora de Rareza (Upgrade)
Si el jugador ya posee un arma o activa, puede aparecer de nuevo en el menú de nivel con una **Rareza Superior**.
*   *Ejemplo*: Tienes una "Pistola Común" -> Al subir de nivel eliges "Pistola Épica".
*   **Efecto**: El arma actual se transforma, recibiendo un incremento masivo en sus estadísticas (Velocidad de disparo, daño, área, etc.) según la rareza.

### 3. Sinergia de Megabonk/Vampire Survivors
Las armas pueden evolucionar si se cumplen ciertos requisitos de nivel y se posee la pasiva adecuada, alcanzando el estado "Megabonk" (Poder Mítico).
