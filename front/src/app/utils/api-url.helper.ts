/**
 * Helper para generar URLs de la API basadas en el entorno
 */
export function getApiBaseUrl(): string {
  // En localhost usar el backend local
  // En producción usar la URL correcta del API
  return window.location.hostname === 'localhost' 
    ? 'http://localhost:56801' 
    : 'https://subastas-api-borox.azurewebsites.net';
}

export function getApiUrl(path: string): string {
  // Asegurar que el path comienza con /
  const normalizedPath = path.startsWith('/') ? path : `/${path}`;
  return `${getApiBaseUrl()}${normalizedPath}`;
}
