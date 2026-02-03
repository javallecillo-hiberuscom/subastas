# 📋 Guía de Testing y Automatización

## 🧪 Testing Automatizado con JavaScript/jQuery

### 1. Script para Testing de Registro

```javascript
// Ejecutar en la consola del navegador en la página de registro
(async function testRegistro() {
    const usuarios = [
        { nombre: 'Test', apellidos: 'Usuario 1', email: 'test1@test.com', password: 'Pass1234' },
        { nombre: 'Test', apellidos: 'Usuario 2', email: 'test2@test.com', password: 'Pass1234' },
        { nombre: 'Test', apellidos: 'Usuario 3', email: 'test3@test.com', password: 'Pass1234' }
    ];

    for (const usuario of usuarios) {
        $('[formcontrolname="nombre"]').val(usuario.nombre).trigger('input');
        $('[formcontrolname="apellidos"]').val(usuario.apellidos).trigger('input');
        $('[formcontrolname="email"]').val(usuario.email).trigger('input');
        $('[formcontrolname="password"]').val(usuario.password).trigger('input');
        
        await new Promise(resolve => setTimeout(resolve, 500));
        
        $('button[type="submit"]').click();
        
        await new Promise(resolve => setTimeout(resolve, 2000));
        console.log(`✅ Usuario ${usuario.email} registrado`);
    }
})();
```

### 2. Script para Testing de Login

```javascript
// Ejecutar en la página de login
async function testLogin(email, password) {
    $('input[type="email"]').val(email).trigger('input');
    $('input[type="password"]').val(password).trigger('input');
    
    await new Promise(resolve => setTimeout(resolve, 300));
    
    $('button[type="submit"]').click();
    
    console.log(`✅ Login intentado para ${email}`);
}

// Uso:
testLogin('lucia@motoriberica.es', 'lucia');
```

### 3. Script para Testing de Validación de Usuarios

```javascript
// Ejecutar en /admin/usuarios como administrador
(async function validarPendientes() {
    const filas = $('tr').filter((i, el) => $(el).text().includes('Pendiente'));
    
    for (let i = 0; i < filas.length; i++) {
        const btnValidar = $(filas[i]).find('button:contains("Validar")');
        if (btnValidar.length) {
            btnValidar.click();
            await new Promise(resolve => setTimeout(resolve, 1500));
            
            // Confirmar si hay modal
            $('button.btn-primary:contains("Validar")').click();
            await new Promise(resolve => setTimeout(resolve, 1000));
            
            console.log(`✅ Usuario ${i+1} validado`);
        }
    }
})();
```

### 4. Script para Testing de Pujas

```javascript
// Ejecutar en la lista de subastas
async function realizarPujas(cantidad, numeroPujas = 5) {
    for (let i = 0; i < numeroPujas; i++) {
        const btnPujar = $('button:contains("Pujar")').first();
        btnPujar.click();
        
        await new Promise(resolve => setTimeout(resolve, 500));
        
        $('input[type="number"]').val(cantidad + (i * 100)).trigger('input');
        
        await new Promise(resolve => setTimeout(resolve, 300));
        
        $('button.btn-primary:contains("Realizar")').click();
        
        await new Promise(resolve => setTimeout(resolve, 2000));
        
        console.log(`✅ Puja ${i+1} realizada por ${cantidad + (i * 100)}€`);
    }
}

// Uso:
realizarPujas(1000, 3); // 3 pujas empezando en 1000€
```

## 📝 Checklist de Testing Manual

### Módulo de Autenticación
- [ ] Registro de usuario nuevo
- [ ] Login con credenciales correctas
- [ ] Login con credenciales incorrectas
- [ ] Cierre de sesión
- [ ] Redirección según rol (admin/usuario)

### Módulo de Usuarios (Admin)
- [ ] Listar todos los usuarios
- [ ] Buscar/filtrar usuarios
- [ ] Validar usuario pendiente
- [ ] Ver notificaciones de nuevos registros
- [ ] Click en notificación navega a usuarios
- [ ] Editar perfil de usuario
- [ ] Eliminar usuario (no último admin)
- [ ] Intentar eliminar último admin (debe fallar)

### Módulo de Empresas (Admin)
- [ ] Crear empresa nueva
- [ ] Editar empresa existente
- [ ] Activar/desactivar empresa
- [ ] Ver actualización de estado en tiempo real
- [ ] Eliminar empresa sin usuarios
- [ ] Intentar eliminar empresa con usuarios

### Módulo de Vehículos (Admin)
- [ ] Crear vehículo nuevo
- [ ] Subir imágenes del vehículo
- [ ] Editar vehículo (verificar formato de fechas)
- [ ] Cambiar estado (Registrado/En Subasta/Vendido)
- [ ] Eliminar vehículo sin subastas
- [ ] Ver galería de imágenes

### Módulo de Subastas
- [ ] Crear subasta nueva
- [ ] Listar subastas activas
- [ ] Ver detalle de subasta
- [ ] Realizar puja (usuario validado)
- [ ] Ver historial de pujas
- [ ] Ver puja ganadora
- [ ] Finalizar subasta automáticamente

### Módulo de Pujas (Usuario)
- [ ] Ver mis pujas
- [ ] Ver estado de pujas (ganador/superado)
- [ ] Recibir notificación de subasta ganada
- [ ] Ver historial completo

### Módulo de Notificaciones
- [ ] Recibir notificación de nuevo registro (admin)
- [ ] Marcar notificación como leída
- [ ] Navegar a usuario desde notificación
- [ ] Marcar todas como leídas
- [ ] Eliminar notificaciones leídas
- [ ] Ver contador de no leídas

## 🔍 Testing de APIs con Postman/cURL

### 1. Registro
```bash
curl -X POST https://tu-backend.azurewebsites.net/api/Usuarios/registro \
  -H "Content-Type: application/json" \
  -d '{
    "nombre": "Test",
    "apellidos": "Automatizado",
    "email": "test@test.com",
    "password": "Test1234",
    "telefono": "666777888",
    "direccion": "Calle Test 123"
  }'
```

### 2. Login
```bash
curl -X POST https://tu-backend.azurewebsites.net/api/Usuarios/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "lucia@motoriberica.es",
    "password": "lucia"
  }'
```

### 3. Validar Usuario (requiere token admin)
```bash
curl -X PUT https://tu-backend.azurewebsites.net/api/Usuarios/2/validar \
  -H "Authorization: Bearer TU_TOKEN_AQUI"
```

## 🤖 Automatización con Selenium (Python)

```python
from selenium import webdriver
from selenium.webdriver.common.by import By
from selenium.webdriver.support.ui import WebDriverWait
from selenium.webdriver.support import expected_conditions as EC
import time

driver = webdriver.Chrome()

def test_registro_completo():
    driver.get("https://tu-app.azurestaticapps.net/registro")
    
    driver.find_element(By.NAME, "nombre").send_keys("Test")
    driver.find_element(By.NAME, "apellidos").send_keys("Selenium")
    driver.find_element(By.NAME, "email").send_keys("selenium@test.com")
    driver.find_element(By.NAME, "password").send_keys("Test1234")
    
    driver.find_element(By.CSS_SELECTOR, "button[type='submit']").click()
    
    # Esperar mensaje de éxito
    wait = WebDriverWait(driver, 10)
    success_msg = wait.until(
        EC.presence_of_element_located((By.CLASS_NAME, "alert-success"))
    )
    
    print("✅ Registro exitoso:", success_msg.text)

def test_login():
    driver.get("https://tu-app.azurestaticapps.net/login")
    
    driver.find_element(By.NAME, "email").send_keys("lucia@motoriberica.es")
    driver.find_element(By.NAME, "password").send_keys("lucia")
    driver.find_element(By.CSS_SELECTOR, "button[type='submit']").click()
    
    time.sleep(2)
    
    # Verificar redirección a dashboard admin
    assert "/admin" in driver.current_url
    print("✅ Login admin exitoso")

# Ejecutar tests
test_registro_completo()
test_login()
driver.quit()
```

## 📊 Testing de Carga

```javascript
// Script para generar múltiples pujas simultáneas
async function testCarga() {
    const promesas = [];
    
    for (let i = 0; i < 50; i++) {
        const puja = fetch('https://tu-backend/api/Pujas', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${token}`
            },
            body: JSON.stringify({
                idSubasta: 1,
                cantidad: 1000 + i
            })
        });
        
        promesas.push(puja);
    }
    
    const resultados = await Promise.allSettled(promesas);
    const exitosos = resultados.filter(r => r.status === 'fulfilled').length;
    
    console.log(`✅ ${exitosos}/50 pujas completadas`);
}
```

## 🛠️ Herramientas Recomendadas

1. **Postman** - Testing de APIs
2. **Selenium** - Automatización de navegador
3. **Jest/Jasmine** - Testing unitario de frontend
4. **xUnit/NUnit** - Testing unitario de backend
5. **Playwright** - Alternativa moderna a Selenium
6. **Artillery** - Testing de carga
