# Playnite – integrar biblioteca del grupo familiar de Steam

Este proyecto implementa un plugin de tipo **GameLibrary** para Playnite que agrega una nueva fuente llamada `Grupo familiar de Steam`. La extensión agrega automáticamente a la colección los juegos pertenecientes a los miembros configurados del grupo familiar y descarga sus metadatos desde la tienda de Steam.

## Características

- Consulta la Web API de Steam (`IPlayerService/GetOwnedGames`) para cada SteamID miembro del grupo familiar.
- Fusiona los catálogos en una base local (`steamFamilyGamesCache.json`) para detectar duplicados y conservar propietarios.
- Registra los juegos en Playnite con la fuente personalizada “Grupo familiar de Steam” y una categoría “Familia Steam” para filtrarlos rápidamente.
- Descarga descripción, carátula y fondo desde `store.steampowered.com` si se mantiene activada la opción “Descargar metadatos automáticamente”.
- Soporta idioma/región configurables y permite reusar la caché cuando Steam no responde.

## Requisitos previos

1. **Clave de la Web API de Steam** (https://steamcommunity.com/dev/apikey).
2. **SteamID 64** (uno por cada integrante del grupo familiar) (https://steamid.io).
3. Playnite 10 u 11 con soporte para extensiones .NET (`RequiresApiVersion: 6.11.0`). Documentación oficial: [Introducción a extensiones](https://api.playnite.link/docs/tutorials/extensions/intro.html).

## Configuración

1. Abre Playnite → `Ajustes` → `Extensiones` → selecciona “Grupo familiar de Steam”.
2. Introduce la clave API y pega los SteamID 64 (se aceptan saltos de línea o comas).
3. Opcional: ajusta idioma/región para metadatos y el retardo entre peticiones.
4. Marca “Descargar metadatos automáticamente” para obtener carátulas y descripciones desde Steam.
5. Guarda los cambios e inicia la importación (`Biblioteca → Actualizar librerías`).

## Estructura del proyecto

- `SteamFamilyLibraryPlugin.cs`: punto de entrada de la librería Playnite.
- `Configuration/`: ajustes, vista y viewmodel para la interfaz de configuración.
- `Services/`: integraciones con la Web API de Steam, almacenamiento en caché y proveedor de metadatos.
- `Models/`: DTOs para transformar las respuestas de Steam.
- `Cache/`: persistencia del catálogo familiar en disco.
- `extension.yaml`: manifiesto requerido por Playnite.

## Construir

```powershell
dotnet restore SteamFamilyLibrary/SteamFamilyLibrary.csproj
dotnet build SteamFamilyLibrary/SteamFamilyLibrary.csproj -c Release
```

## Instalación

### Opción 1: copia manual

1. Crea una carpeta dentro de `Playnite/Extensions`, por ejemplo `SteamFamilyGroup`.
2. Copia **todos** los archivos generados en `SteamFamilyLibrary/bin/Release/net472/` (SteamFamilyLibrary.dll y el resto de dependencias `Newtonsoft.Json.dll`, `System.Text.Json.dll`, etc.).
3. Copia `extension.yaml` a la misma carpeta.
4. Reinicia Playnite o usa `Para desarrolladores → Recargar scripts`.

### Opción 2: archivo `.pext`

En la pestaña **Releases** de este repositorio encontrarás un `.pext` empaquetado (ej. `SteamFamilyGroup-v0.1.0-beta.pext`). Descárgalo y ejecutalo dandole doble click.

