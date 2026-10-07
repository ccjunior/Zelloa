# Zelloa — API Specification

**Documento:** 06-Zelloa-API-Specification  
**Versão:** 1.0  
**Status:** Especificação inicial da API do MVP  
**Produto:** Zelloa  
**Backend:** .NET 10 / ASP.NET Core  
**Arquitetura:** Modular Monolith + Clean Architecture + Vertical Slice Architecture

---

# 1. Objetivo

Este documento define os princípios, convenções e contratos principais da API da Zelloa.

A API será o ponto central de autoridade para:

- autenticação;
- autorização;
- isolamento entre escolas;
- regras de negócio;
- catálogo;
- alunos e responsáveis;
- pedidos;
- pagamentos;
- operação da cantina;
- administração escolar;
- auditoria;
- dashboard.

Os frontends:

```text
Zelloa Family
Zelloa School
```

consumirão a mesma API.

---

# 2. Princípio Fundamental

O backend é a autoridade sobre:

```text
Tenant
Usuário
Permissões
Aluno
Produtos
Preços
Disponibilidade
Pedidos
Valores
Pagamentos
Status
```

O frontend nunca deverá ser considerado fonte confiável para decisões de negócio ou segurança.

---

# 3. Base URL

Exemplo de produção:

```text
https://api.zelloa.com.br
```

Ambientes poderão utilizar:

```text
Development
Staging
Production
```

---

# 4. Convenção de Rotas

Rotas REST orientadas a recursos.

Exemplos:

```text
/api/schools
/api/classrooms
/api/students
/api/products
/api/orders
/api/payments
```

Evitar rotas baseadas em verbos quando uma representação REST clara existir.

---

# 5. Versionamento

Não introduzir versionamento complexo antecipadamente.

O MVP poderá iniciar com:

```text
/api/...
```

Quando houver necessidade real de contratos incompatíveis:

```text
/api/v2/...
```

ou estratégia equivalente poderá ser adotada.

Não criar `v1` apenas por convenção sem necessidade arquitetural.

---

# 6. JSON

Formato padrão:

```text
application/json
```

Convenção de propriedades:

```text
camelCase
```

Exemplo:

```json
{
  "studentId": "...",
  "displayName": "João",
  "classroomName": "1º Ano A"
}
```

---

# 7. Datas

Datas e horários transmitidos pela API deverão utilizar ISO 8601.

Exemplo:

```text
2026-10-05T14:30:00-03:00
```

Internamente:

```text
DateTimeOffset
```

quando a informação representar instante temporal.

---

# 8. Valores Monetários

Valores monetários:

```json
{
  "amount": 15.50,
  "currency": "BRL"
}
```

Backend utiliza:

```text
decimal
```

Nunca:

```text
float
double
```

para valores financeiros.

---

# 9. Identificadores

Utilizar identificadores não sequenciais quando apropriado.

Exemplo:

```text
Guid
```

Não expor IDs incrementais caso isso aumente risco de enumeração sem necessidade.

---

# 10. Autenticação

Endpoints privados exigem usuário autenticado.

Utilizar ASP.NET Core Identity com sessão por cookie autenticado. O cookie deverá ser `HttpOnly`, `Secure` em produção e possuir política `SameSite` explícita. Não emitir JWT próprio nem integrar provedor OIDC nesta fase.

Configurar CORS somente para origins autorizadas e com suporte a credenciais. Endpoints que alteram estado deverão validar token antifalsificação enviado no header `X-XSRF-TOKEN`.

O tenant é extraído da identidade autenticada emitida pelo servidor; um `TenantId` enviado pelo cliente nunca substitui esse contexto. Cada conta institucional pertence a uma instituição, e uma instituição pode ter várias contas; `PlatformAdmin` é global e não possui tenant.

A API deverá possuir abstração equivalente a:

```text
ICurrentUser
```

com informações necessárias como:

```text
UserId
TenantId
Roles
```

quando aplicável.

---

# 11. Autorização

Não utilizar somente:

```text
if (user.Role == ...)
```

espalhado pelo código.

Utilizar policies.

Exemplos:

```text
CanManageSchool
CanManageCatalog
CanOperateCafeteria
CanViewStudent
CanCreateOrder
```

---

# 12. Roles

Papéis iniciais:

```text
PlatformAdmin
SchoolAdmin
CafeteriaOperator
Guardian
```

Role não substitui validação de tenant ou ownership.

---

# 13. Tenant

Tenant deverá ser determinado pelo contexto autenticado.

Nunca confiar cegamente em:

```json
{
  "tenantId": "..."
}
```

enviado pelo cliente.

---

# 14. Tenant em Rotas

Evitar rotas do tipo:

```text
/api/tenants/{tenantId}/orders
```

para operações normais do usuário autenticado quando o tenant já puder ser determinado pelo contexto.

Isso reduz risco de acesso cruzado.

---

# 15. PlatformAdmin

Operações internas de plataforma poderão necessitar identificação explícita de tenant.

Essas operações deverão possuir autorização específica.

Não reutilizar endpoints comuns de SchoolAdmin para administração global.

---

# 16. Ownership

Guardian poderá acessar somente alunos aos quais estiver validamente associado.

Exemplo:

```text
Guardian
   │
   └── GuardianStudent
             │
             ▼
          Student
```

Possuir `StudentId` não concede acesso.

---

# 17. DTOs

Entidades EF Core ou Domain não deverão ser retornadas diretamente pela API.

Utilizar:

```text
Request DTO
Response DTO
Projection
```

adequados ao caso de uso.

---

# 18. Validação

Entradas externas deverão ser validadas.

FluentValidation poderá ser utilizado.

Exemplo:

```text
CreateProductRequest
    ↓
Validator
    ↓
Command
```

Validação de formato não substitui invariantes de domínio.

---

# 19. Erros

Utilizar Problem Details conforme RFC 9457.

Exemplo conceitual:

```json
{
  "type": "...",
  "title": "Validation failed",
  "status": 400,
  "detail": "...",
  "instance": "...",
  "traceId": "..."
}
```

---

# 20. Códigos HTTP

Utilizar semanticamente:

```text
200 OK
201 Created
204 No Content
400 Bad Request
401 Unauthorized
403 Forbidden
404 Not Found
409 Conflict
422 Unprocessable Content
429 Too Many Requests
500 Internal Server Error
```

Não retornar sempre `200`.

---

# 21. Segurança contra Enumeração

Quando apropriado, recursos de outro tenant ou sem ownership poderão ser tratados como:

```text
404 Not Found
```

em vez de revelar sua existência.

A estratégia deverá ser consistente.

---

# 22. Paginação

Listagens potencialmente grandes deverão ser paginadas.

Exemplo:

```text
GET /api/students?page=1&pageSize=20
```

Resposta:

```json
{
  "items": [],
  "page": 1,
  "pageSize": 20,
  "totalCount": 0
}
```

---

# 23. Limites de Paginação

Backend deverá impor:

```text
DefaultPageSize
MaximumPageSize
```

Não permitir:

```text
?pageSize=1000000
```

---

# 24. Ordenação e Filtros

Somente filtros e campos suportados explicitamente deverão ser aceitos.

Evitar mecanismos genéricos capazes de construir consultas arbitrárias.

---

# 25. OpenAPI

A API deverá disponibilizar OpenAPI nos ambientes apropriados.

A documentação deve refletir:

- endpoints;
- requests;
- responses;
- autenticação;
- códigos HTTP;
- schemas.

---

# 26. API Client Angular

Quando a API estiver suficientemente estável, avaliar geração do client TypeScript a partir do OpenAPI.

Preferível a manter manualmente dezenas de interfaces duplicadas.

---

# 27. Correlation ID

Cada request deverá possuir identificador rastreável.

Exemplo:

```text
CorrelationId
```

Deverá aparecer nos logs e, quando apropriado, nos erros.

---

# 28. Health Checks

Endpoints de saúde deverão existir.

Exemplo:

```text
/health
```

Poderão evoluir para:

```text
/health/live
/health/ready
```

quando infraestrutura justificar.

---

# 29. Rate Limiting

Aplicar especialmente em:

- autenticação;
- criação de pedidos;
- criação de pagamentos;
- webhooks quando apropriado.

Configuração deverá considerar o fluxo real.

---

# 30. Idempotência

Operações financeiras críticas deverão avaliar idempotência.

Principalmente:

```text
CreateOrder
CreatePayment
Webhook
Refund
```

Quando necessário, utilizar:

```text
Idempotency-Key
```

ou mecanismo equivalente.

---

# 31. SCHOOL

## 31.1 Criar escola

Operação administrativa de plataforma.

```text
POST /api/schools
```

Autorização:

```text
PlatformAdmin
```

Request conceitual:

```json
{
  "name": "Escola Exemplo",
  "tradeName": "Escola Exemplo",
  "identifier": "...",
  "contactEmail": "secretaria@escola.example",
  "contactPhone": "+5571999990000",
  "timezone": "America/Bahia"
}
```

`contactEmail`, `contactPhone` e `identifier` são opcionais. Não armazenar dados pessoais de alunos além do necessário à operação.

Resposta:

```text
201 Created
```

A operação cria o Tenant de isolamento e sua primeira School na mesma transação. O `PlatformAdmin` deverá então gerar o convite do administrador escolar inicial:

```text
POST /api/schools/{schoolId}/administrator-invitation
```

Autorização: `PlatformAdmin`. Request: `displayName` e `email`. A resposta contém um `activationUrl` de uso único, entregue manualmente ao administrador. A URL expira em 24 horas.

---

# 32. Consultar Escola Atual

```text
GET /api/school
```

Autorização:

```text
SchoolAdmin
CafeteriaOperator
```

conforme necessidade.

Não exigir TenantId enviado pelo cliente.

---

# 33. Atualizar Escola

```text
PUT /api/school
```

Autorização:

```text
CanManageSchool
```

Somente campos explicitamente editáveis.

---

# 34. CLASSROOMS

## Criar turma

```text
POST /api/classrooms
```

Autorização:

```text
CanManageSchool
```

Request:

```json
{
  "name": "1º Ano A",
  "academicYear": 2026
}
```

---

# 35. Listar Turmas

```text
GET /api/classrooms
```

Filtros possíveis:

```text
academicYear
status
search
page
pageSize
```

---

# 36. Consultar Turma

```text
GET /api/classrooms/{classroomId}
```

Obrigatório validar tenant.

---

# 37. Atualizar Turma

```text
PUT /api/classrooms/{classroomId}
```

---

# 38. Status da Turma

Preferir ação explícita quando representar transição de domínio.

Exemplo:

```text
PATCH /api/classrooms/{classroomId}/status
```

Request:

```json
{
  "status": "Inactive"
}
```

---

# 39. STUDENTS

## Criar aluno

```text
POST /api/students
```

Autorização:

```text
CanManageSchool
```

Request conceitual:

```json
{
  "name": "João Silva",
  "classroomId": "..."
}
```

Aplicar minimização de dados.

---

# 40. Dados do Aluno

No MVP, evitar exigir:

- CPF;
- endereço;
- informações médicas;
- documentos pessoais;

sem necessidade funcional concreta.

---

# 41. Listar Alunos

```text
GET /api/students
```

Filtros:

```text
classroomId
status
search
page
pageSize
```

---

# 42. Consultar Aluno

```text
GET /api/students/{studentId}
```

SchoolAdmin:

```text
tenant ownership
```

Guardian:

```text
guardian ownership
```

---

# 43. Atualizar Aluno

```text
PUT /api/students/{studentId}
```

Autorização:

```text
CanManageSchool
```

---

# 44. GUARDIANS

## Criar responsável

Fluxo administrativo:

```text
POST /api/guardians
```

Autorização:

```text
CanManageSchool
```

Request conceitual:

```json
{
  "displayName": "Maria Silva",
  "email": "maria@example.com"
}
```

A operação cria a conta pendente com papel `Guardian`, cria o perfil Guardian e retorna um `activationUrl` individual de uso único para entrega manual. A conta não acessa alunos até que a escola crie um vínculo explícito. Respostas contendo links de ativação usam `Cache-Control: no-store`.

Consultar responsável:

```text
GET /api/guardians/{guardianId}
```

Autorização: `CanManageSchool`; o registro deverá pertencer ao tenant atual.

---

# 45. Vincular Responsável

```text
POST /api/students/{studentId}/guardians/{guardianId}
```

Autorização:

```text
CanManageSchool
```

A operação deverá ser idempotente quando apropriado.

---

# 46. Remover Vínculo

```text
DELETE /api/students/{studentId}/guardians/{guardianId}
```

Não excluir necessariamente o Guardian.

Remove associação.

---

# 47. Alunos do Responsável Atual

Endpoint importante para Zelloa Family:

```text
GET /api/me/students
```

Resposta:

```json
[
  {
    "studentId": "...",
    "name": "João",
    "classroom": {
      "id": "...",
      "name": "1º Ano A"
    }
  }
]
```

---

# 48. CATEGORIES

## Criar categoria

```text
POST /api/catalog/categories
```

Exemplo:

```json
{
  "name": "Salgados"
}
```

Autorização:

```text
CanManageCatalog
```

Resposta: `201 Created` com `id`, `name` e `isActive` (inicialmente `true`).

## Atualizar categoria

```text
PUT /api/catalog/categories/{categoryId}
```

Request:

```json
{
  "name": "Salgados"
}
```

## Alterar status da categoria

```text
PATCH /api/catalog/categories/{categoryId}/status
```

Request: `{ "status": "Active" }` ou `{ "status": "Inactive" }`.

As operações de criação, atualização e alteração de status exigem antifalsificação.

---

# 49. Listar Categorias

Administração:

```text
GET /api/catalog/categories
```

Autorização: `CanManageCatalog`. Aceita `status` (`Active` ou `Inactive`), `search`, `page` e `pageSize`. A resposta é paginada e pode incluir categorias inativas conforme o filtro.

---

# 50. PRODUCTS

## Criar Produto

```text
POST /api/catalog/products
```

Request:

```json
{
  "name": "Pão de queijo",
  "description": "Porção individual",
  "categoryId": "...",
  "price": 5.50,
  "imageUrl": "https://cdn.example.com/products/pao-de-queijo.jpg",
  "available": true
}
```

Backend valida categoria/tenant.

`description` e `imageUrl` são opcionais. `imageUrl`, quando informado, deve ser uma URL HTTPS absoluta; upload e armazenamento de arquivos não fazem parte desta fase. `price` é um valor decimal em BRL, maior que zero, com até duas casas decimais. O produto inicia ativo.

---

# 51. Atualizar Produto

```text
PUT /api/catalog/products/{productId}
```

Autorização: `CanManageCatalog`. Atualiza `name`, `description`, `categoryId`, `price` e `imageUrl`; status e disponibilidade são alterados em operações próprias.

Preço alterado não modifica snapshots de pedidos anteriores.

---

# 52. Alterar Disponibilidade

```text
PATCH /api/catalog/products/{productId}/availability
```

Exemplo:

```json
{
  "available": false
}
```

Autorização: `CanManageCatalog`; exige antifalsificação.

## Alterar status do produto

```text
PATCH /api/catalog/products/{productId}/status
```

Request: `{ "status": "Active" }` ou `{ "status": "Inactive" }`.

Autorização: `CanManageCatalog`; exige antifalsificação.

Listagem e consulta administrativa usam `CanManageCatalog`. A listagem aceita `categoryId`, `status`, `availability`, `search`, `page` e `pageSize`, e retorna resposta paginada.

---

# 53. Catálogo para Family

Endpoint otimizado para consumo do responsável:

```text
GET /api/catalog/available
```

Autorização: usuário autenticado no papel `Guardian`. Retorna somente categorias ativas com produtos ativos e disponíveis; não retornar produtos indisponíveis.

Resposta conceitual:

```json
{
  "categories": [
    {
      "id": "...",
      "name": "Salgados",
      "products": [
        {
          "id": "...",
          "name": "Pão de queijo",
          "description": "Porção individual",
          "price": 5.50,
          "imageUrl": "https://cdn.example.com/products/pao-de-queijo.jpg"
        }
      ]
    }
  ]
}
```

---

# 54. ORDERS

## Criar Pedido

```text
POST /api/orders
```

Autorização:

```text
CanCreateOrder
```

Request:

```json
{
  "studentId": "...",
  "items": [
    {
      "productId": "...",
      "quantity": 2
    }
  ]
}
```

O responsável não envia uma data de entrega. A data operacional é definida pelo servidor como a data local corrente da instituição. O turno é derivado da turma do aluno.

Antes de criar o pedido, a API verifica o limite configurado pela escola para aquele turno. Se não houver configuração, responder `409 OrderCutoffNotConfigured`; se o limite tiver passado no timezone institucional, responder `409 OrderCutoffPassed`.

### Configurar limites por turno

```text
GET /api/school/order-cutoffs
PUT /api/school/order-cutoffs/{shift}
```

Autorização: `SchoolAdmin` do tenant atual. O `{shift}` deve corresponder ao rótulo de turno utilizado nas turmas (ignorando espaços externos e diferenças entre maiúsculas/minúsculas).

Request do `PUT`:

```json
{
  "cutoffTime": "08:30"
}
```

O valor é uma hora local da escola no formato `HH:mm`, sem offset. Uma configuração é obrigatória para cada turno antes de aceitar pedidos desse turno.

---

# 55. O Frontend Não Envia Preço

Não aceitar como autoridade:

```json
{
  "productId": "...",
  "quantity": 2,
  "unitPrice": 0.01
}
```

Mesmo que `unitPrice` apareça por alguma razão de UX, deverá ser ignorado como fonte financeira.

Preferível não enviá-lo.

---

# 56. Processamento do Pedido

Backend:

```text
identifica Guardian
      ↓
valida Student
      ↓
valida vínculo
      ↓
valida Tenant
      ↓
carrega Products
      ↓
valida disponibilidade
      ↓
obtém preços atuais
      ↓
calcula total
      ↓
cria snapshots
      ↓
persiste Order
```

---

# 57. Resposta do Pedido

```json
{
  "orderId": "...",
  "student": {
    "id": "...",
    "name": "João"
  },
  "status": "AwaitingPayment",
  "items": [
    {
      "productId": "...",
      "productName": "Pão de queijo",
      "unitPrice": 5.50,
      "quantity": 2,
      "subtotal": 11.00
    }
  ],
  "total": 11.00,
  "createdAt": "..."
}
```

---

# 58. Consultar Pedido

```text
GET /api/orders/{orderId}
```

Guardian:

```text
somente pedidos de alunos associados
```

School:

```text
somente tenant atual
```

---

# 59. Histórico do Responsável

```text
GET /api/me/orders
```

Filtros:

```text
studentId
status
dateFrom
dateTo
page
pageSize
```

---

# 60. Repetir Pedido

Possível endpoint:

```text
POST /api/orders/{orderId}/repeat
```

O backend não clona cegamente.

Deverá revalidar:

```text
Guardian
Student
Products
Availability
Current Prices
Cutoff
Business Rules
```

---

# 61. Cancelar Pedido

```text
POST /api/orders/{orderId}/cancel
```

Permitido somente conforme estado/regra.

Pagamento confirmado exige tratamento específico.

---

# 62. PAYMENTS

## Criar cobrança Pix

```text
POST /api/orders/{orderId}/payments/pix
```

Autorização:

```text
Guardian proprietário do pedido
```

Em ambiente de desenvolvimento com `Payments:SimulationEnabled=true`, esse endpoint cria uma cobrança simulada. A resposta inclui `isSimulated: true`; os dados Pix são marcadores de simulação, não podem ser pagos e não representam QR Code bancário.

---

# 63. Request de Pagamento

Idealmente nenhum valor financeiro precisa vir do frontend.

Exemplo:

```json
{}
```

O backend encontra:

```text
Order.Total
```

---

# 64. Resposta Pix

```json
{
  "paymentId": "...",
  "orderId": "...",
  "amount": 11.00,
  "currency": "BRL",
  "status": "Pending",
  "pixCopyPaste": "...",
  "qrCode": "...",
  "expiresAt": "...",
  "isSimulated": false,
  "simulatedAt": null
}
```

---

# 65. Consultar Pagamento

```text
GET /api/payments/{paymentId}
```

Resposta:

```json
{
  "paymentId": "...",
  "orderId": "...",
  "status": "Pending",
  "amount": 11.00,
  "expiresAt": "...",
  "confirmedAt": null,
  "isSimulated": false,
  "simulatedAt": null
}
```

A resposta inclui `isSimulated` e `simulatedAt`. Quando `status` for `SimulatedConfirmed`, `confirmedAt` permanece `null` e o pedido continua `AwaitingPayment`.

### Operações de simulação (somente Development)

Estas rotas são registradas apenas quando o ambiente da API é `Development` e `Payments:SimulationEnabled=true`. São protegidas por sessão de responsável e antiforgery:

```text
POST /api/development/payments/{paymentId}/confirm
POST /api/development/payments/{paymentId}/expire
```

Ambas retornam a representação do pagamento com `isSimulated: true`. A confirmação usa `SimulatedConfirmed`, não marca o pedido como pago e não atende ao Gate da Fase 5. Em qualquer outro ambiente, essas rotas não existem. A API falha ao iniciar se a simulação estiver habilitada fora de `Development`.

---

# 66. Segurança do Payment

Possuir `paymentId` não concede acesso.

Validar:

```text
Guardian → Student → Order → Payment
```

ou autorização correspondente da escola.

---

# 67. WEBHOOK

```text
POST /api/webhooks/payments/{provider}
```

Esse endpoint não utiliza autenticação comum de Guardian/School.

Autenticação segue provider.

---

# 68. Resposta do Webhook

Responder rapidamente conforme contrato do provider.

Processamento deverá seguir requisitos específicos da instituição.

Não retornar informações internas desnecessárias.

---

# 69. Webhook Duplicado

Receber evento duplicado deverá continuar produzindo resposta válida ao provider, sem repetir efeitos financeiros.

---

# 70. CAFETERIA

## Fila

```text
GET /api/cafeteria/orders
```

Autorização:

```text
CanOperateCafeteria
```

Por padrão, retornar somente pedidos operacionalmente relevantes.

---

# 71. Filtros da Cantina

Possíveis filtros:

```text
status
classroomId
studentName
date
```

---

# 72. Resposta da Fila

```json
{
  "items": [
    {
      "orderId": "...",
      "student": {
        "id": "...",
        "name": "João"
      },
      "classroom": {
        "id": "...",
        "name": "1º Ano A"
      },
      "status": "Paid",
      "items": [
        {
          "name": "Pão de queijo",
          "quantity": 2
        }
      ],
      "paidAt": "..."
    }
  ]
}
```

---

# 73. Iniciar Preparação

```text
POST /api/cafeteria/orders/{orderId}/prepare
```

Transição:

```text
Paid
 ↓
Preparing
```

---

# 74. Marcar Pronto

```text
POST /api/cafeteria/orders/{orderId}/ready
```

Transição:

```text
Preparing
 ↓
Ready
```

---

# 75. Entregar

```text
POST /api/cafeteria/orders/{orderId}/deliver
```

Transição:

```text
Ready
 ↓
Delivered
```

Registrar:

```text
DeliveredAt
DeliveredBy
```

---

# 76. Transições Inválidas

Exemplo:

```text
AwaitingPayment
      ↓
Delivered
```

deverá ser rejeitado.

Possível resposta:

```text
409 Conflict
```

---

# 77. DASHBOARD

```text
GET /api/dashboard/today
```

Autorização:

```text
SchoolAdmin
```

Resposta conceitual:

```json
{
  "date": "2026-10-05",
  "orders": {
    "total": 120,
    "awaitingPayment": 10,
    "paid": 20,
    "preparing": 15,
    "ready": 5,
    "delivered": 65,
    "cancelled": 5
  },
  "confirmedAmount": 1250.50
}
```

---

# 78. Dashboard Não Deve Carregar Entidades

Utilizar queries/projections agregadas.

Não carregar milhares de Orders em memória para calcular contadores.

---

# 79. PAYMENT CONFIGURATION

Endpoint administrativo futuro/inicial conforme necessidade:

```text
GET /api/settings/payments
```

Autorização:

```text
SchoolAdmin
```

---

# 80. Configuração de Provider

Possível:

```text
PUT /api/settings/payments
```

Entretanto, credenciais bancárias sensíveis não deverão ser tratadas como configuração comum.

O fluxo definitivo depende do provider.

---

# 81. Credenciais

Nunca retornar:

```text
ClientSecret
API Key
Private Key
Certificate private material
```

pela API administrativa.

---

# 82. AUDIT

Consulta de auditoria poderá ser adicionada conforme necessidade.

Exemplo:

```text
GET /api/audit
```

Restrito a usuários autorizados.

Não expor indiscriminadamente logs técnicos.

---

# 83. USER CONTEXT

Endpoint útil:

```text
GET /api/me
```

Autenticação:

```text
GET  /api/auth/csrf
POST /api/auth/login
POST /api/auth/logout
```

`GET /api/auth/csrf` emite os valores necessários para proteção antifalsificação. Login e logout exigem o token no header `X-XSRF-TOKEN`. Login recebe email e senha e estabelece o cookie autenticado; logout invalida a sessão. Não há cadastro público de contas.

A ativação de contas convidadas é feita por:

```text
POST /api/auth/activate
```

Request: `email`, `token` e `password`. A ativação exige o token antifalsificação no header `X-XSRF-TOKEN`; a senha é definida pelo próprio convidado. O token é aleatório, armazenado somente como hash, de uso único e expira em 24 horas. A resposta e o link de ativação deverão usar `Cache-Control: no-store`. O link é entregue manualmente pelo administrador que iniciou o convite.

O primeiro `PlatformAdmin` é criado por bootstrap único no backend, usando credenciais secretas de ambiente; não há credenciais padrão no repositório.

Os endpoints de autenticação e convite deverão aplicar `Cache-Control: no-store` às respostas sensíveis. Credenciais inválidas deverão produzir resposta genérica sem revelar se a conta existe.

Resposta:

```json
{
  "userId": "...",
  "displayName": "...",
  "tenantId": "...",
  "roles": [
    "Guardian"
  ]
}
```

`tenantId` é `null` para `PlatformAdmin`. Requisições sem sessão válida recebem `401 Unauthorized`.

Não retornar claims internas desnecessárias.

---

# 84. FAMILY — Jornada Principal

```text
GET /api/me
        ↓
GET /api/me/students
        ↓
GET /api/catalog/available
        ↓
POST /api/orders
        ↓
POST /api/orders/{id}/payments/pix
        ↓
GET /api/payments/{id}
        ↓
GET /api/orders/{id}
```

Essa é a principal jornada mobile do MVP.

---

# 85. SCHOOL — Jornada Administrativa

```text
Login
 ↓
GET /api/school
 ↓
Classrooms
 ↓
Students
 ↓
Guardians
 ↓
Catalog
 ↓
Dashboard
```

---

# 86. CAFETERIA — Jornada Operacional

```text
Login
 ↓
GET /api/cafeteria/orders
 ↓
Prepare
 ↓
Ready
 ↓
Deliver
```

---

# 87. Não Criar Endpoint Genérico

Evitar:

```text
POST /api/entities
POST /api/execute
POST /api/action
```

Cada endpoint deverá representar caso de uso claro.

---

# 88. Minimal APIs

Endpoints deverão ser pequenos.

Exemplo conceitual:

```csharp
group.MapPost("/", async (
    CreateOrderRequest request,
    CreateOrderHandler handler,
    CancellationToken ct) =>
{
    var result = await handler.HandleAsync(request, ct);

    return result.ToHttpResult();
});
```

Detalhes concretos poderão variar.

---

# 89. Endpoint Não Contém Regra de Negócio

Evitar:

```csharp
if (product.Price > ...)
{
    ...
}
```

no endpoint.

Regras pertencem ao caso de uso/domínio apropriado.

---

# 90. Vertical Slice

Estrutura exemplo:

```text
Application/
└── Orders/
    └── CreateOrder/
        ├── Command.cs
        ├── Handler.cs
        ├── Validator.cs
        └── Response.cs
```

API:

```text
Api/
└── Endpoints/
    └── Orders/
        └── CreateOrderEndpoint.cs
```

ou organização equivalente.

---

# 91. Queries

Queries deverão buscar somente dados necessários.

Preferir:

```text
Select(...)
```

em vez de carregar agregados inteiros para telas somente leitura.

---

# 92. AsNoTracking

Queries somente leitura utilizando EF Core deverão usar:

```text
AsNoTracking()
```

quando apropriado.

---

# 93. N+1

Evitar consultas N+1.

Principalmente:

```text
Students
Orders
OrderItems
Products
Cafeteria Queue
```

---

# 94. Cache

Não introduzir Redis inicialmente apenas para acelerar endpoints.

Primeiro:

```text
query adequada
índice adequado
projection adequada
paginação
```

Adicionar cache quando métricas justificarem.

---

# 95. Upload de Comprovante

Não deverá existir endpoint:

```text
POST /api/payments/{id}/receipt
```

para confirmação de Pix.

Isso contradiz o objetivo central da Zelloa.

---

# 96. Marcar Como Pago Manualmente

Não deverá existir:

```text
POST /api/orders/{id}/mark-paid
```

disponível para Guardian ou operador comum.

Confirmação financeira deve vir de fonte confiável.

---

# 97. Dados Sensíveis

Responses deverão seguir minimização.

Exemplo:

Cantina precisa saber:

```text
Nome do aluno
Turma
Pedido
```

Não precisa conhecer:

```text
CPF do responsável
dados bancários
informações pessoais adicionais
```

---

# 98. CORS

Permitir somente origins necessários por ambiente.

Exemplo:

```text
https://app.zelloa.com.br
https://school.zelloa.com.br
```

Não utilizar:

```text
AllowAnyOrigin
```

em produção sem justificativa.

---

# 99. HTTPS

Produção deverá operar exclusivamente sobre HTTPS.

---

# 100. Headers de Segurança

Aplicar headers apropriados conforme arquitetura e deployment.

Não confiar somente no frontend.

---

# 101. Request Size

Definir limites apropriados.

A API do MVP não necessita receber uploads grandes.

---

# 102. Timeout

Integrações externas deverão possuir timeout explícito.

CancellationToken deverá ser propagado quando apropriado.

---

# 103. Retry

Não adicionar retry indiscriminadamente.

Especial cuidado com:

```text
CreatePayment
Refund
```

para evitar duplicidade financeira.

---

# 104. Resilience

Quando necessário, utilizar políticas apropriadas para:

- timeout;
- transient failures;
- circuit breaking;

sempre considerando semântica da operação.

---

# 105. Logs

Logs devem ser estruturados.

Exemplo:

```text
Order {OrderId} created for Student {StudentId}
```

Não:

```text
"Pedido criado: " + order.ToString()
```

---

# 106. Dados Proibidos em Logs

Não registrar:

```text
Passwords
Access Tokens
Client Secrets
Private Keys
Full authorization headers
Sensitive banking credentials
```

---

# 107. Testes da API

Cada feature crítica deverá possuir testes adequados.

Priorizar:

```text
Integration Tests
Application Tests
Domain Tests
Architecture Tests
```

---

# 108. Testcontainers

Integrações com PostgreSQL deverão preferencialmente utilizar banco real efêmero via Testcontainers.

Evitar confiar exclusivamente em provider EF InMemory para comportamento relacional.

---

# 109. Testes de Tenant

Obrigatórios:

```text
Tenant A cannot read Tenant B
Tenant A cannot update Tenant B
Tenant A cannot delete Tenant B
Tenant A cannot operate Tenant B orders
```

---

# 110. Testes de Guardian

Obrigatórios:

```text
Guardian A cannot access Student B
Guardian A cannot order for Student B
Guardian A cannot access Order B
Guardian A cannot access Payment B
```

---

# 111. Testes Financeiros

Obrigatórios:

```text
client cannot change price
client cannot change total
invalid webhook cannot confirm
duplicate webhook is idempotent
amount mismatch cannot confirm
expired payment cannot be treated incorrectly
concurrent confirmation is safe
```

---

# 112. Status Codes nos Testes

Integration Tests deverão validar não somente payload, mas:

```text
status code
authorization
side effects
database state
tenant isolation
```

---

# 113. API Contract

Quando frontend depender de contrato existente, mudanças incompatíveis deverão ser tratadas conscientemente.

Não alterar silenciosamente:

```text
field name
type
meaning
endpoint
status code
```

---

# 114. Breaking Changes

Antes de breaking change:

1. identificar consumidores;
2. avaliar impacto;
3. atualizar documentação;
4. atualizar testes;
5. atualizar frontends.

---

# 115. OpenAPI como Contrato Executável

À medida que o sistema evoluir, OpenAPI deverá se tornar referência executável dos contratos HTTP.

Este documento define princípios e endpoints essenciais.

OpenAPI define a implementação concreta vigente.

---

# 116. Endpoints Principais do MVP

Resumo:

```text
IDENTITY

GET    /api/auth/csrf
POST   /api/auth/login
POST   /api/auth/logout
POST   /api/auth/activate
GET    /api/me


SCHOOL

POST   /api/schools
POST   /api/schools/{schoolId}/administrator-invitation
GET    /api/school
PUT    /api/school


CLASSROOMS

POST   /api/classrooms
GET    /api/classrooms
GET    /api/classrooms/{id}
PUT    /api/classrooms/{id}
PATCH  /api/classrooms/{id}/status


STUDENTS

POST   /api/students
GET    /api/students
GET    /api/students/{id}
PUT    /api/students/{id}
PATCH  /api/students/{id}/status


GUARDIANS

POST   /api/guardians
GET    /api/guardians/{id}
POST   /api/students/{studentId}/guardians/{guardianId}
DELETE /api/students/{studentId}/guardians/{guardianId}

GET    /api/me/students


CATALOG

POST   /api/catalog/categories
GET    /api/catalog/categories
PUT    /api/catalog/categories/{id}
PATCH  /api/catalog/categories/{id}/status

POST   /api/catalog/products
GET    /api/catalog/products
GET    /api/catalog/products/{id}
PUT    /api/catalog/products/{id}
PATCH  /api/catalog/products/{id}/availability
PATCH  /api/catalog/products/{id}/status

GET    /api/catalog/available


ORDERS

POST   /api/orders
GET    /api/orders/{id}
GET    /api/me/orders
POST   /api/orders/{id}/repeat
POST   /api/orders/{id}/cancel


PAYMENTS

POST   /api/orders/{id}/payments/pix
GET    /api/payments/{id}


WEBHOOKS

POST   /api/webhooks/payments/{provider}


CAFETERIA

GET    /api/cafeteria/orders
POST   /api/cafeteria/orders/{id}/prepare
POST   /api/cafeteria/orders/{id}/ready
POST   /api/cafeteria/orders/{id}/deliver


DASHBOARD

GET    /api/dashboard/today


SETTINGS

GET    /api/settings/payments
PUT    /api/settings/payments


HEALTH

GET    /health
```

A lista não constitui obrigação de implementar todos simultaneamente.

A ordem de implementação é determinada pelo Documento 03.

---

# 117. Dependência Entre Endpoints

Não implementar endpoint apenas porque consta neste documento.

Exemplo:

```text
Fase 2
→ Students

Fase 3
→ Catalog

Fase 4
→ Orders

Fase 5
→ Payments
```

Respeitar fases.

---

# 118. Definition of Done de um Endpoint

Um endpoint somente será considerado pronto quando:

```text
✓ caso de uso implementado
✓ request validado
✓ autorização aplicada
✓ tenant validado
✓ ownership validado quando necessário
✓ regra de negócio protegida
✓ resposta adequada
✓ ProblemDetails adequado
✓ status HTTP correto
✓ logs apropriados
✓ testes implementados
✓ OpenAPI atualizado
✓ build sem warnings
```

---

# 119. Regras para a IA

Ao implementar endpoints:

1. consultar este documento;
2. consultar a fase atual;
3. implementar somente endpoints pertencentes à fase;
4. não antecipar endpoints futuros;
5. não alterar contratos silenciosamente;
6. não criar abstrações genéricas sem necessidade;
7. não colocar regra de negócio no endpoint;
8. não confiar em IDs recebidos para autorização;
9. não confiar em preços enviados;
10. não avançar para próxima fase automaticamente.

---

# 120. Princípio Final da API

A API da Zelloa deverá ser:

```text
Segura
Previsível
Tenant-aware
Testável
Observável
Simples
Orientada a casos de uso
```

A API existe para proteger e executar as regras do produto.

Não deverá ser apenas uma camada CRUD sobre o banco de dados.

---

**Fim do documento — Zelloa API Specification v1.0**
