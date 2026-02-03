# Casos de Uso - Sistema de Subastas

## 👥 Actores del Sistema

### 1. Usuario Registrado
Usuario que se ha registrado en el sistema pero aún no ha sido validado.

### 2. Usuario Validado
Usuario registrado cuyo documento IAE ha sido aprobado por un administrador.

### 3. Administrador
Usuario con permisos especiales para gestionar el sistema.

---

## 📋 Casos de Uso por Actor

### 🆕 Usuario Registrado (No Validado)

#### CU-01: Registro de Usuario
**Descripción:** Un nuevo usuario se registra en el sistema proporcionando sus datos personales y empresariales.

**Precondiciones:** 
- El usuario no está registrado previamente
- Tiene un CIF válido de empresa

**Flujo Principal:**
1. Usuario accede a la página de registro
2. Completa el formulario con:
   - Datos personales (nombre, apellidos, email, teléfono)
   - Datos de empresa (CIF, nombre comercial, dirección)
   - Credenciales (email, password)
3. El sistema valida los datos
4. El sistema crea la cuenta en estado "No Validado"
5. Usuario recibe confirmación de registro

**Postcondiciones:**
- Usuario creado con estado "registrado"
- Usuario puede iniciar sesión pero no puede realizar pujas

---

#### CU-02: Subir Documento IAE
**Descripción:** Usuario registrado sube su documento IAE para validación.

**Precondiciones:**
- Usuario está registrado y logueado
- No ha subido documento IAE previamente o fue rechazado

**Flujo Principal:**
1. Usuario navega a "Subir IAE"
2. Selecciona archivo (PDF, JPG, PNG hasta 10MB)
3. Hace clic en "Subir Documento"
4. Sistema valida el archivo
5. Sistema almacena el documento
6. Usuario recibe confirmación

**Postcondiciones:**
- Documento IAE almacenado en sistema
- Administrador puede ver y validar el documento

---

#### CU-03: Ver Subastas
**Descripción:** Usuario puede ver todas las subastas activas pero no puede pujar.

**Precondiciones:**
- Usuario está logueado

**Flujo Principal:**
1. Usuario accede al dashboard
2. Ve lista de subastas activas
3. Puede ver detalles de cada vehículo
4. Ve mensaje indicando que debe ser validado para pujar

**Postcondiciones:**
- Usuario informado del estado de su cuenta

---

### ✅ Usuario Validado

#### CU-04: Realizar Puja
**Descripción:** Usuario validado realiza una puja en una subasta activa.

**Precondiciones:**
- Usuario está validado
- Subasta está activa
- Puja supera el precio actual + incremento mínimo

**Flujo Principal:**
1. Usuario accede a detalle de vehículo
2. Ve precio actual y incremento mínimo
3. Ingresa cantidad a pujar
4. Sistema valida que:
   - Usuario está validado
   - Cantidad >= precio actual + incremento mínimo
   - Subasta está activa
5. Sistema registra la puja
6. Actualiza precio actual de la subasta
7. Envía notificación a usuarios interesados

**Flujo Alternativo (Puja Rechazada):**
5a. Sistema detecta que la cantidad es insuficiente
5b. Muestra error indicando el mínimo requerido

**Postcondiciones:**
- Puja registrada en sistema
- Precio actual actualizado
- Notificaciones enviadas

---

#### CU-05: Ver Mis Pujas
**Descripción:** Usuario ve histórico de sus pujas y su estado.

**Precondiciones:**
- Usuario está validado y logueado

**Flujo Principal:**
1. Usuario accede a "Mis Pujas"
2. Sistema muestra lista de pujas del usuario con:
   - Vehículo
   - Cantidad pujada
   - Estado (ganando/superado)
   - Fecha de puja
3. Usuario puede filtrar por estado

**Postcondiciones:**
- Usuario informado del estado de sus pujas

---

#### CU-06: Actualizar Perfil
**Descripción:** Usuario actualiza su información personal.

**Precondiciones:**
- Usuario está logueado

**Flujo Principal:**
1. Usuario accede a "Perfil"
2. Modifica datos permitidos:
   - Nombre, apellidos
   - Teléfono, dirección
   - Foto de perfil
   - Contraseña (opcional)
3. Guarda cambios
4. Sistema valida y actualiza datos

**Postcondiciones:**
- Datos de usuario actualizados

---

### 🔐 Administrador

#### CU-07: Validar Usuario
**Descripción:** Administrador revisa y valida la cuenta de un usuario.

**Precondiciones:**
- Usuario ha subido documento IAE
- Administrador está logueado

**Flujo Principal:**
1. Admin accede a "Gestión de Usuarios"
2. Filtra usuarios "Pendientes de Validación"
3. Selecciona usuario a validar
4. Revisa documento IAE
5. Si documento es válido:
   - Hace clic en "Validar Usuario"
   - Sistema cambia estado a "Validado"
   - Usuario recibe notificación de validación

**Flujo Alternativo (Documento Inválido):**
5a. Admin rechaza validación
5b. Envía mensaje al usuario
5c. Usuario debe subir nuevo documento

**Postcondiciones:**
- Usuario validado puede realizar pujas
- Notificación enviada al usuario

---

#### CU-08: Gestionar Vehículos
**Descripción:** Administrador crea, edita y elimina vehículos del catálogo.

**Precondiciones:**
- Administrador está logueado

**Flujo Principal (Crear):**
1. Admin accede a "Gestión de Vehículos"
2. Hace clic en "Nuevo Vehículo"
3. Completa formulario con:
   - Datos técnicos (marca, modelo, año, km)
   - Características (motor, carrocería, transmisión)
   - Fechas (matriculación, ITV)
   - Documentación
4. Sube imágenes del vehículo
5. Sistema valida y crea vehículo

**Flujo Principal (Editar):**
1. Admin selecciona vehículo existente
2. Modifica datos necesarios
3. Guarda cambios
4. Sistema actualiza vehículo

**Flujo Principal (Eliminar):**
1. Admin selecciona vehículo
2. Confirma eliminación
3. Sistema marca vehículo como inactivo

**Postcondiciones:**
- Vehículo creado/actualizado/eliminado
- Disponible para crear subastas

---

#### CU-09: Crear Subasta
**Descripción:** Administrador crea una nueva subasta para un vehículo.

**Precondiciones:**
- Vehículo existe en sistema
- Vehículo no tiene subasta activa

**Flujo Principal:**
1. Admin accede a gestión de subastas
2. Selecciona vehículo
3. Define parámetros:
   - Fecha inicio y fin
   - Precio inicial
   - Incremento mínimo
4. Activa subasta
5. Sistema publica subasta

**Postcondiciones:**
- Subasta creada y visible para usuarios
- Usuarios pueden empezar a pujar

---

#### CU-10: Gestionar Empresas
**Descripción:** Administrador gestiona el catálogo de empresas.

**Precondiciones:**
- Administrador está logueado

**Flujo Principal:**
1. Admin accede a "Gestión de Empresas"
2. Puede:
   - Crear nueva empresa
   - Editar datos de empresa
   - Activar/Desactivar empresa
   - Asignar empresas a usuarios

**Postcondiciones:**
- Catálogo de empresas actualizado

---

#### CU-11: Ver Dashboard Administrativo
**Descripción:** Administrador visualiza estadísticas y métricas del sistema.

**Precondiciones:**
- Administrador está logueado

**Flujo Principal:**
1. Admin accede al dashboard
2. Ve estadísticas generales:
   - Total usuarios (validados/pendientes)
   - Total empresas
   - Total vehículos
   - Subastas activas/terminadas
   - Gráficos de pujas y subastas
3. Ve listados de:
   - Subastas activas con detalle
   - Subastas terminadas con ganador

**Postcondiciones:**
- Administrador informado del estado del sistema

---

#### CU-12: Gestionar Notificaciones
**Descripción:** Administrador puede enviar y gestionar notificaciones del sistema.

**Precondiciones:**
- Administrador está logueado

**Flujo Principal:**
1. Admin accede a "Notificaciones Admin"
2. Ve notificaciones del sistema
3. Puede marcar como leídas
4. Puede enviar notificaciones personalizadas

**Postcondiciones:**
- Notificaciones gestionadas

---

## 🔄 Flujos de Trabajo Completos

### Flujo 1: Registro y Primera Puja

```
1. Usuario se registra (CU-01)
   ↓
2. Usuario sube IAE (CU-02)
   ↓
3. Usuario ve subastas mientras espera (CU-03)
   ↓
4. Admin valida usuario (CU-07)
   ↓
5. Usuario recibe notificación
   ↓
6. Usuario realiza primera puja (CU-04)
   ↓
7. Usuario monitorea en "Mis Pujas" (CU-05)
```

### Flujo 2: Ciclo de Vida de una Subasta

```
1. Admin crea vehículo (CU-08)
   ↓
2. Admin crea subasta (CU-09)
   ↓
3. Usuarios realizan pujas (CU-04)
   ↓
4. Sistema actualiza precio actual
   ↓
5. Subasta finaliza automáticamente
   ↓
6. Sistema determina ganador
   ↓
7. Admin ve resultado en dashboard (CU-11)
```

---

## 🚫 Restricciones del Sistema

### Reglas de Negocio

1. **RN-01:** Solo usuarios validados pueden realizar pujas
2. **RN-02:** Administradores NO pueden realizar pujas
3. **RN-03:** Cada puja debe superar precio actual + incremento mínimo
4. **RN-04:** Un vehículo solo puede tener una subasta activa
5. **RN-05:** Documento IAE es obligatorio para validación
6. **RN-06:** Usuario con cuenta inactiva no puede acceder al sistema
7. **RN-07:** Solo se aceptan documentos IAE en formatos PDF, JPG, PNG hasta 10MB
8. **RN-08:** Las subastas terminan automáticamente al llegar a fecha fin
9. **RN-09:** No se pueden modificar pujas una vez realizadas
10. **RN-10:** El email debe ser único en el sistema

---

*Documento actualizado: 3 de febrero de 2026*
