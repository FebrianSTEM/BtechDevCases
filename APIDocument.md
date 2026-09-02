# AuthWallet API Documentation

> **Base URL:** `https://<your-domain>`  
> **Version:** 1.0  
> **Format:** JSON (`Content-Type: application/json`)

---

## Table of Contents

- [Authentication](#authentication)
  - [Register](#1-register)
  - [Login](#2-login)
  - [Refresh Token](#3-refresh-token)
  - [Logout](#4-logout)
- [User](#user)
  - [Get Current User](#5-get-current-user)
- [Wallet](#wallet)
  - [Get Wallet Info](#6-get-wallet-info)
  - [Transfer Funds](#7-transfer-funds)
- [Schemas](#schemas)

---

## Authentication

### 1. Register

**`POST`** `/api/auth/register`

Mendaftarkan pengguna baru ke sistem.

#### Request Body

| Field             | Type     | Required | Constraints                                                                 |
|-------------------|----------|----------|-----------------------------------------------------------------------------|
| `email`           | `string` | ✅ Yes   | Format email yang valid                                                     |
| `password`        | `string` | ✅ Yes   | Min. 8 karakter, harus mengandung huruf besar, huruf kecil, angka, & simbol |
| `confirmPassword` | `string` | ✅ Yes   | Min. 1 karakter                                                             |

**Password Pattern:** `^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^A-Za-z0-9]).{8,}$`

#### Example Request

```json
{
  "email": "user@example.com",
  "password": "MyP@ssw0rd",
  "confirmPassword": "MyP@ssw0rd"
}
```

#### Response

| Status | Description |
|--------|-------------|
| `200`  | OK – Registrasi berhasil |

---

### 2. Login

**`POST`** `/api/auth/login`

Autentikasi pengguna dan mendapatkan token akses.

#### Request Body

| Field      | Type     | Required | Constraints            |
|------------|----------|----------|------------------------|
| `email`    | `string` | ✅ Yes   | Format email yang valid |
| `password` | `string` | ✅ Yes   | Min. 1 karakter        |

#### Example Request

```json
{
  "email": "user@example.com",
  "password": "MyP@ssw0rd"
}
```

#### Response

| Status | Description |
|--------|-------------|
| `200`  | OK – Login berhasil, mengembalikan access token & refresh token |

---

### 3. Refresh Token

**`POST`** `/api/auth/refresh`

Memperbarui access token menggunakan refresh token yang masih valid.

#### Request Body

| Field          | Type     | Required | Constraints     |
|----------------|----------|----------|-----------------|
| `refreshToken` | `string` | ✅ Yes   | Min. 1 karakter |

#### Example Request

```json
{
  "refreshToken": "your-refresh-token-here"
}
```

#### Response

| Status | Description |
|--------|-------------|
| `200`  | OK – Mengembalikan access token baru |

---

### 4. Logout

**`POST`** `/api/auth/logout`

Mencabut refresh token dan mengakhiri sesi pengguna.

#### Request Body

| Field          | Type     | Required | Constraints     |
|----------------|----------|----------|-----------------|
| `refreshToken` | `string` | ✅ Yes   | Min. 1 karakter |

#### Example Request

```json
{
  "refreshToken": "your-refresh-token-here"
}
```

#### Response

| Status | Description |
|--------|-------------|
| `200`  | OK – Logout berhasil, refresh token dicabut |

---

## User

### 5. Get Current User

**`GET`** `/api/me`

Mengambil informasi profil pengguna yang sedang login.

> 🔒 **Requires Authentication** – Sertakan `Authorization: Bearer <access_token>` di header.

#### Request Body

Tidak diperlukan.

#### Response

| Status | Description |
|--------|-------------|
| `200`  | OK – Mengembalikan data profil pengguna saat ini |

---

## Wallet

### 6. Get Wallet Info

**`GET`** `/api/wallet`

Mengambil informasi saldo dan detail wallet pengguna.

> 🔒 **Requires Authentication** – Sertakan `Authorization: Bearer <access_token>` di header.

#### Request Body

Tidak diperlukan.

#### Response

| Status | Description |
|--------|-------------|
| `200`  | OK – Mengembalikan informasi wallet pengguna |

---

### 7. Transfer Funds

**`POST`** `/api/wallet/transfer`

Melakukan transfer saldo ke pengguna lain berdasarkan email penerima.

> 🔒 **Requires Authentication** – Sertakan `Authorization: Bearer <access_token>` di header.

#### Request Body

| Field            | Type      | Required | Constraints                            |
|------------------|-----------|----------|----------------------------------------|
| `recipientEmail` | `string`  | ✅ Yes   | Format email yang valid                |
| `amount`         | `number`  | ✅ Yes   | Nilai minimum: `0.01` (double)         |
| `notes`          | `string`  | ❌ No    | Nullable, catatan opsional             |
| `idempotencyKey` | `string`  | ✅ Yes   | Min. 1 karakter, mencegah duplikasi    |

> **Idempotency Key:** Gunakan nilai unik (misalnya UUID) untuk setiap transaksi guna mencegah double transfer jika terjadi retry.

#### Example Request

```json
{
  "recipientEmail": "recipient@example.com",
  "amount": 50000.00,
  "notes": "Pembayaran tagihan bulan ini",
  "idempotencyKey": "550e8400-e29b-41d4-a716-446655440000"
}
```

#### Response

| Status | Description |
|--------|-------------|
| `200`  | OK – Transfer berhasil diproses |

---

## Schemas

### `RegisterRequest`

```json
{
  "email": "string (email, required)",
  "password": "string (min 8 chars, pattern: uppercase + lowercase + digit + symbol, required)",
  "confirmPassword": "string (required)"
}
```

---

### `LoginRequest`

```json
{
  "email": "string (email, required)",
  "password": "string (required)"
}
```

---

### `RefreshTokenRequest`

```json
{
  "refreshToken": "string (required)"
}
```

---

### `TransferRequest`

```json
{
  "recipientEmail": "string (email, required)",
  "amount": "number (double, min: 0.01, required)",
  "notes": "string (nullable, optional)",
  "idempotencyKey": "string (required)"
}
```

---

> 📌 **Catatan:** Semua endpoint yang memerlukan autentikasi harus menyertakan header berikut:
> ```
> Authorization: Bearer <access_token>
> ```
