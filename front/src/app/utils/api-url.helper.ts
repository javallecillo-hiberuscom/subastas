/**
 * Helper para generar URLs de la API basadas en el entorno
 */
export function getApiBaseUrl(): string {
  return window.location.hostname === 'localhost' 
    ? 'http://localhost:56801' 
    : 'https://subastaswebapi20260202162157-f3frc5dfgdata6cx.canadacentral-01.azurewebsites.net';
}

export function getApiUrl(path: string): string {
  // Asegurar que el path comienza con /
  const normalizedPath = path.startsWith('/') ? path : `/${path}`;
  return `${getApiBaseUrl()}${normalizedPath}`;
}
