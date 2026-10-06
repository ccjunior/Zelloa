# Zelloa — Technical Architecture

**Documento:** 02-Zelloa-Technical-Architecture  
**Versão:** 1.2  
**Status:** Arquitetura aprovada para o MVP  
**Produto:** Zelloa  
**Backend:** .NET 10 / ASP.NET Core  
**Frontend:** Angular  
**Banco de Dados:** PostgreSQL  
**Arquitetura:** Clean Architecture + Vertical Slice Architecture  
**Modelo de Deployment:** Modular Monolith  
**Aplicações Frontend:** Zelloa Family + Zelloa School

---

# 1. Objetivo

Este documento define a arquitetura técnica inicial da plataforma Zelloa.

A arquitetura deverá priorizar:

- organização por domínio e funcionalidade;
- baixo acoplamento;
- alta coesão;
- testabilidade;
- segurança;
- manutenibilidade;
- clareza para desenvolvedores e agentes de IA;
- evolução incremental;
- observabilidade;
- simplicidade operacional;
- isolamento entre instituições;
- separação entre experiência dos responsáveis e operação escolar.

A arquitetura deverá seguir um princípio fundamental:

> Complexidade arquitetural somente deverá ser introduzida quando resolver um problema real.

---

# 2. Decisão Arquitetural Principal

A Zelloa será construída inicialmente como um:

**Modular Monolith**

utilizando:

- Clean Architecture para definição das fronteiras e dependências;
- Vertical Slice Architecture para organização dos casos de uso;
- Domain-Driven Design pragmático para regras relevantes de negócio;
- CQRS pragmático para separação entre comandos e consultas;
- Angular para os frontends;
- PWA para a experiência dos responsáveis;
- ASP.NET Core para API;
- PostgreSQL para persistência.

Não serão utilizados microservices no MVP.

---

# 3. Aplicações da Plataforma

A plataforma será composta inicialmente por três aplicações principais.

## 3.1 Zelloa API

Backend central da plataforma.

Tecnologia:

**.NET 10 / ASP.NET Core**

Responsável por:

- autenticação;
- autorização;
- regras de negócio;
- multi-tenancy;
- persistência;
- pedidos;
- pagamentos;
- integração Pix;
- catálogo;
- alunos;
- responsáveis;
- operação da cantina;
- auditoria;
- integrações externas.

A API será a autoridade sobre todas as regras críticas.

---

# 4. Zelloa Family

Aplicação destinada aos responsáveis.

Tecnologia:

**Angular + PWA**

Características:

- mobile-first;
- instalável;
- otimizada para celulares;
- interface simples;
- baixa quantidade de interações;
- foco em pedidos e acompanhamento.

Principais funcionalidades:

- autenticação;
- seleção do aluno;
- visualização do cardápio;
- carrinho;
- criação de pedido;
- pagamento Pix;
- acompanhamento;
- histórico;
- repetição de pedido.

Fluxo principal:

```text
Login
  ↓
Selecionar aluno
  ↓
Cardápio
  ↓
Carrinho
  ↓
Pedido
  ↓
Pix
  ↓
Pagamento confirmado
  ↓
Acompanhamento
```

---

# 5. PWA

O Zelloa Family será desenvolvido como Progressive Web App.

Objetivos:

- permitir instalação na tela inicial;
- oferecer experiência próxima a um aplicativo;
- permitir atualizações sem publicação em lojas;
- reduzir barreira de entrada;
- preparar a plataforma para funcionalidades futuras de PWA.

Entretanto:

> Zelloa Family não será offline-first no MVP.

Operações críticas dependerão obrigatoriamente da API.

Nunca considerar dados provenientes exclusivamente de cache como autoridade para:

- pagamento;
- preço;
- disponibilidade;
- criação de pedido;
- cancelamento;
- status financeiro.

O Service Worker deverá possuir estratégia conservadora para dados transacionais.

---

# 6. Zelloa School

Aplicação destinada à escola e à operação da cantina.

Tecnologia:

**Angular**

Características:

- desktop-first;
- tablet-friendly;
- responsiva;
- orientada a operação administrativa.

Usuários:

- SchoolAdmin;
- CafeteriaOperator.

Principais funcionalidades:

- dashboard;
- pedidos;
- fila da cantina;
- alunos;
- responsáveis;
- turmas;
- produtos;
- categorias;
- disponibilidade;
- configurações;
- consulta de pagamentos.

---

# 7. Separação dos Frontends

Zelloa Family e Zelloa School serão aplicações distintas.

Não utilizar:

```text
/app/guardian
/app/admin
```

dentro de uma única aplicação Angular.

A separação existe porque os contextos possuem:

- usuários diferentes;
- jornadas diferentes;
- UX diferente;
- autorização diferente;
- componentes diferentes;
- ciclos de evolução diferentes.

---

# 8. Workspace Frontend

As aplicações deverão existir dentro de um workspace compartilhado.

Estrutura conceitual:

```text
web/
│
├── apps/
│   │
│   ├── zelloa-family/
│   │
│   └── zelloa-school/
│
└── libs/
    │
    ├── api-client/
    ├── authentication/
    ├── shared-ui/
    └── utilities/
```

O mecanismo específico de workspace poderá ser definido durante a criação do frontend.

Não introduzir ferramentas adicionais sem necessidade concreta.

---

# 9. Compartilhamento entre Frontends

Somente código realmente compartilhável deverá existir em `libs`.

Exemplos:

```text
api-client
authentication
shared-ui
utilities
```

Evitar transformar `shared` em depósito genérico.

Exemplo:

```text
ShoppingCart
```

pertence ao:

```text
zelloa-family
```

Enquanto:

```text
CafeteriaQueue
```

pertence ao:

```text
zelloa-school
```

---

# 10. Backoffice Zelloa

No futuro poderá existir uma terceira aplicação:

```text
Zelloa Backoffice
```

destinada à administração interna da plataforma.

Possíveis responsabilidades:

- tenants;
- suporte;
- integrações;
- monitoramento;
- configuração de clientes.

Essa aplicação está:

> FORA DO ESCOPO DO MVP.

Não deverá ser criada antecipadamente.

---

# 11. Visão Geral

```text
             ┌─────────────────────┐
             │    Zelloa Family    │
             │    Angular PWA      │
             └──────────┬──────────┘
                        │
                        │ HTTPS
                        │
               ┌────────▼─────────┐
               │                  │
               │    Zelloa API    │
               │     .NET 10      │
               │                  │
               └────────┬─────────┘
                        │
              ┌─────────┴───────────┐
              │                     │
              ▼                     ▼
       ┌────────────┐        ┌─────────────┐
       │ PostgreSQL │        │     PSP     │
       └────────────┘        │    / Pix    │
                             └─────────────┘
                        ▲
                        │
                        │ HTTPS
                        │
             ┌──────────┴──────────┐
             │    Zelloa School    │
             │       Angular       │
             └─────────────────────┘
```

---

# 12. Estrutura Geral do Repositório

```text
Zelloa/
│
├── src/
│   ├── Zelloa.Domain/
│   ├── Zelloa.Application/
│   ├── Zelloa.Infrastructure/
│   └── Zelloa.Api/
│
├── tests/
│   ├── Zelloa.Domain.Tests/
│   ├── Zelloa.Application.Tests/
│   ├── Zelloa.IntegrationTests/
│   └── Zelloa.ArchitectureTests/
│
├── web/
│   ├── apps/
│   │   ├── zelloa-family/
│   │   └── zelloa-school/
│   │
│   └── libs/
│       ├── api-client/
│       ├── authentication/
│       ├── shared-ui/
│       └── utilities/
│
├── docs/
├── docker/
├── docker-compose.yml
├── Directory.Build.props
├── Directory.Packages.props
└── Zelloa.sln
```

---

# 13. Clean Architecture

As dependências deverão apontar para dentro.

```text
┌──────────────────────────────┐
│          Zelloa.Api          │
│         Presentation         │
├──────────────────────────────┤
│     Zelloa.Application       │
│       Vertical Slices        │
├──────────────────────────────┤
│        Zelloa.Domain         │
│         Core Domain          │
└──────────────────────────────┘

       Infrastructure
            │
            ├────► Application
            └────► Domain
```

---

# 14. Domain

Projeto:

```text
Zelloa.Domain
```

Responsável pelo núcleo do negócio.

Deverá conter:

- entidades;
- Aggregate Roots;
- Value Objects;
- regras de domínio;
- invariantes;
- Domain Events;
- enums;
- exceções de domínio quando necessárias.

Não deverá conhecer:

- ASP.NET Core;
- Entity Framework;
- PostgreSQL;
- Angular;
- HTTP;
- PSP;
- controllers/endpoints;
- DTOs de transporte.

---

# 15. Contextos Funcionais

Contextos iniciais:

```text
Schools
Academics
Catalog
Orders
Payments
Identity
Cafeteria
```

Esses contextos representam agrupamento lógico.

Não representam microservices.

---

# 16. Entidades Iniciais

```text
Tenant
School
Classroom
Student
Guardian
GuardianStudent

Category
Product

Order
OrderItem

Payment
PaymentWebhookEvent

User
AccountInvitation

AuditEntry
```

Entidades adicionais somente deverão ser criadas quando necessárias para casos de uso definidos.

---

# 17. Aggregate Roots

Candidatos iniciais:

```text
School
Student
Product
Order
Payment
```

Aggregate Roots deverão existir para representar consistência e comportamento.

Não deverão simplesmente reproduzir tabelas.

---

# 18. Entidades com Comportamento

Evitar:

```csharp
order.Status = OrderStatus.Paid;
```

Preferir:

```csharp
order.MarkAsPaid(paymentId, paidAt);
```

Exemplo:

```csharp
public void MarkAsPaid(Guid paymentId, DateTimeOffset paidAt)
{
    if (Status != OrderStatus.AwaitingPayment)
        throw new DomainException(
            "Order cannot be marked as paid.");

    PaymentId = paymentId;
    PaidAt = paidAt;
    Status = OrderStatus.Paid;
}
```

A entidade deverá proteger suas invariantes.

---

# 19. Value Objects

Utilizar quando representarem conceitos reais.

Possíveis exemplos:

```text
Money
Email
PhoneNumber
PixTransactionId
```

Não criar Value Objects apenas por padrão arquitetural.

---

# 20. Valores Financeiros

Nunca utilizar:

```text
float
double
```

para valores financeiros.

Utilizar:

```text
decimal
```

ou Value Object equivalente.

Moeda inicial:

```text
BRL
```

---

# 21. Application

Projeto:

```text
Zelloa.Application
```

Será organizado por funcionalidades.

Evitar estrutura horizontal:

```text
Services/
Repositories/
DTOs/
Commands/
Queries/
Validators/
```

Preferir Vertical Slice Architecture.

---

# 22. Vertical Slice Architecture

Exemplo:

```text
Application/
│
├── Orders/
│   ├── CreateOrder/
│   │   ├── Command.cs
│   │   ├── Handler.cs
│   │   ├── Validator.cs
│   │   └── Response.cs
│   │
│   ├── GetOrder/
│   │   ├── Query.cs
│   │   ├── Handler.cs
│   │   └── Response.cs
│   │
│   └── CancelOrder/
│
├── Catalog/
│   ├── CreateProduct/
│   ├── UpdateProduct/
│   ├── GetProducts/
│   └── SetProductAvailability/
│
└── Payments/
    ├── CreatePixCharge/
    ├── ProcessPaymentWebhook/
    └── GetPaymentStatus/
```

---

# 23. Regra dos Slices

Cada caso de uso deverá permanecer o mais autocontido possível.

Um slice poderá possuir:

```text
CreateOrder/
├── Command.cs
├── Handler.cs
├── Validator.cs
├── Response.cs
└── Mapping.cs
```

Nem todo slice precisa possuir todos esses arquivos.

Não criar arquivos vazios para obedecer a template.

---

# 24. CQRS

Commands representam alteração de estado.

Exemplos:

```text
CreateOrderCommand
CancelOrderCommand
StartPreparationCommand
MarkOrderAsReadyCommand
MarkOrderAsDeliveredCommand
```

Queries representam leitura.

Exemplos:

```text
GetOrderQuery
GetGuardianOrdersQuery
GetCafeteriaQueueQuery
GetDailyDashboardQuery
```

Queries não alteram estado.

Commands não devem ser utilizados como mecanismo de consulta.

---

# 25. Mediator

Poderá ser utilizado um mediator para dispatch de Commands e Queries.

Entretanto:

> A arquitetura não dependerá conceitualmente de uma biblioteca específica.

Handlers representam casos de uso.

A biblioteca será apenas um detalhe de implementação.

---

# 26. Validação

FluentValidation poderá ser utilizado.

Fluxo:

```text
Command
   ↓
Validator
   ↓
Handler
   ↓
Domain
```

Diferenciar:

## Input Validation

Exemplo:

```text
StudentId obrigatório
ProductId obrigatório
Quantity > 0
```

## Domain Invariant

Exemplo:

```text
Pedido entregue não pode retornar para Preparing.
```

Invariantes pertencem ao domínio.

---

# 27. Persistência

Tecnologias:

```text
Entity Framework Core
PostgreSQL
```

O DbContext ficará em:

```text
Zelloa.Infrastructure
```

Estrutura:

```text
Infrastructure/
└── Persistence/
    ├── ZelloaDbContext.cs
    ├── Configurations/
    ├── Migrations/
    └── Interceptors/
```

---

# 28. Repository Pattern

Não utilizar Generic Repository.

Evitar:

```csharp
IRepository<T>
Repository<T>
```

apenas para encapsular EF Core.

EF Core já fornece:

```text
DbContext
DbSet
```

Repositories específicos poderão existir somente quando houver justificativa de domínio.

Exemplo:

```csharp
IOrderRepository
```

---

# 29. Database Abstraction

A Application não deverá depender da implementação concreta do DbContext.

Poderá existir uma abstração mínima:

```csharp
public interface IZelloaDbContext
{
    DbSet<Order> Orders { get; }
    DbSet<Product> Products { get; }

    Task<int> SaveChangesAsync(
        CancellationToken cancellationToken);
}
```

A interface definitiva deverá expor somente o necessário.

---

# 30. Queries

Consultas deverão priorizar:

```text
AsNoTracking
Projection
Select
Pagination
```

Evitar carregar Aggregate Roots completos apenas para produzir modelos de leitura.

---

# 31. Multi-Tenancy

A Zelloa será multi-tenant desde o MVP.

Cada instituição será representada por:

```text
TenantId
```

Dados de uma escola não poderão ser acessados por outra.

---

# 32. Tenant Context

A Application deverá possuir abstração semelhante a:

```csharp
public interface ITenantContext
{
    Guid TenantId { get; }
}
```

O TenantId deverá ser resolvido pelo contexto autenticado.

Nunca confiar em um `TenantId` arbitrário enviado pelo frontend.

Para usuários institucionais, o `TenantId` será incluído pelo servidor na identidade autenticada após validar a associação da conta. O cliente não poderá selecionar ou sobrescrever esse valor em uma requisição.

---

# 33. Isolamento entre Tenants

Utilizar defesa em profundidade.

Possíveis mecanismos:

```text
Global Query Filters
Tenant-aware queries
Explicit validation
Database constraints
```

Testes deverão comprovar isolamento.

---

# 34. Identity

Perfis iniciais:

```text
PlatformAdmin
SchoolAdmin
CafeteriaOperator
Guardian
```

Aplicações:

```text
Guardian
   ↓
Zelloa Family

SchoolAdmin
CafeteriaOperator
   ↓
Zelloa School
```

`PlatformAdmin` será reservado para administração interna.

Não haverá frontend próprio para esse perfil no MVP.

## Estratégia de autenticação

Utilizar ASP.NET Core Identity para contas, credenciais e papéis, com sessão por cookie autenticado.

O cookie de sessão deverá utilizar `HttpOnly`, `Secure` em produção e `SameSite` apropriado. Endpoints que alteram estado deverão exigir proteção antifalsificação. A API deverá permitir credenciais somente para origins configuradas explicitamente.

Não emitir JWT próprio nem depender de provedor OIDC externo nesta fase. Não implementar criptografia ou armazenamento de senhas próprios.

O primeiro `PlatformAdmin` poderá ser criado por bootstrap único usando email e senha fornecidos somente por configuração secreta de ambiente. O bootstrap não deverá definir credenciais padrão nem registrar a senha em logs. Aplicar a migration de Identity antes de configurar o bootstrap.

O `PlatformAdmin` cria o tenant e a escola e gera um convite para ativar a conta do administrador escolar inicial. O administrador escolar gera convites para responsáveis. Contas convidadas são criadas sem senha e sem acesso até a ativação; a pessoa define sua senha ao aceitar o convite.

Convites são links individuais de uso único, válidos por 24 horas no MVP. O emissor copia e entrega o link manualmente. Não integrar serviço de e-mail ou outro provedor de entrega nesta fase. Armazenar somente representação protegida do token; não registrar tokens nos logs nem armazená-los em claro. A ativação confirma o endereço de e-mail informado no convite.

Persistir a expiração e o consumo do convite em registro associado à conta, tenant e papel concedido. A ativação e o consumo do convite deverão ocorrer atomicamente para impedir reutilização concorrente.

## Associação de conta e instituição

Cada conta institucional pertence a exatamente um tenant, e um tenant pode possuir várias contas institucionais. Essa cardinalidade permite que diferentes responsáveis tenham contas próprias na mesma instituição, conforme a Especificação do Produto, seção 10.

Representar a associação com `TenantId` anulável na conta; não criar índice único sobre `TenantId`. `PlatformAdmin` é uma conta global e não pertence a um tenant. O `TenantId` institucional é copiado para a identidade autenticada pelo servidor e consumido por `ITenantContext`.

---

# 35. Autorização

Utilizar policies.

Exemplos:

```text
CanManageSchool
CanManageCatalog
CanOperateCafeteria
CanViewStudent
CanCreateOrder
```

Evitar:

```csharp
if (user.Role == "Admin")
```

espalhado pelo sistema.

A autorização deverá considerar:

```text
User
Role
Tenant
Resource Ownership
```

---

# 36. API

Projeto:

```text
Zelloa.Api
```

Responsável por:

- bootstrap;
- DI;
- endpoints;
- autenticação;
- autorização;
- middleware;
- OpenAPI;
- health checks;
- configuração HTTP.

---

# 37. Minimal APIs

Preferência:

```text
ASP.NET Core Minimal APIs
```

Endpoints deverão ser pequenos.

Exemplo:

```csharp
group.MapPost("/", async (
    CreateOrderCommand command,
    ISender sender,
    CancellationToken ct) =>
{
    var result = await sender.Send(command, ct);

    return Results.Ok(result);
});
```

Não colocar regra de negócio nos endpoints.

---

# 38. Organização dos Endpoints

Organizar por feature/contexto.

Exemplo:

```text
Endpoints/
├── OrdersEndpoints.cs
├── CatalogEndpoints.cs
├── PaymentsEndpoints.cs
├── StudentsEndpoints.cs
└── CafeteriaEndpoints.cs
```

Alternativamente, slices poderão registrar seus endpoints diretamente.

A abordagem escolhida deverá permanecer consistente.

---

# 39. Result Pattern

Falhas esperadas não deverão depender de Exceptions.

Utilizar abordagem equivalente a:

```text
Result<T>
```

Tipos possíveis:

```text
Validation
NotFound
Conflict
Forbidden
BusinessRule
```

Exceptions deverão representar situações verdadeiramente excepcionais.

---

# 40. Problem Details

Erros HTTP deverão seguir:

```text
RFC 9457 Problem Details
```

Exemplo:

```json
{
  "type": "...",
  "title": "Product unavailable",
  "status": 409,
  "detail": "...",
  "traceId": "..."
}
```

Stack traces nunca deverão ser retornados em produção.

---

# 41. Payments

Payments será separado de Orders.

Regra:

> Orders não conhece detalhes do PSP.

Fluxo:

```text
Order
  ↓
Application
  ↓
IPaymentGateway
  ↓
Infrastructure
  ↓
PSP
```

---

# 42. Payment Gateway

A Application poderá definir:

```csharp
public interface IPaymentGateway
{
    Task<CreatePixChargeResult> CreatePixChargeAsync(
        CreatePixChargeRequest request,
        CancellationToken cancellationToken);

    Task<PaymentStatusResult> GetPaymentStatusAsync(
        string externalTransactionId,
        CancellationToken cancellationToken);
}
```

A implementação ficará na Infrastructure.

---

# 43. Estrutura do Provider

```text
Infrastructure/
└── Payments/
    └── ProviderName/
        ├── ProviderPaymentGateway.cs
        ├── ProviderOptions.cs
        ├── ProviderClient.cs
        └── Models/
```

O MVP utilizará apenas um PSP.

Não criar suporte completo a múltiplos providers antecipadamente.

---

# 44. Webhooks

Endpoint conceitual:

```text
POST /api/webhooks/payments/{provider}
```

Nunca:

```text
Webhook
   ↓
Order.Status = Paid
```

Fluxo correto:

```text
Webhook
   ↓
Validar autenticidade
   ↓
Identificar evento
   ↓
Verificar idempotência
   ↓
Localizar Payment
   ↓
Validar valor/transação
   ↓
Confirmar Payment
   ↓
Atualizar Order
   ↓
Persistir atomicamente
```

---

# 45. Idempotência

Obrigatória em operações financeiras.

Estrutura possível:

```text
PaymentWebhookEvent

Id
Provider
ExternalEventId
ReceivedAt
ProcessedAt
Status
PayloadHash
```

Deverá existir constraint impedindo processamento duplicado.

---

# 46. Transações Financeiras

Operações relacionadas deverão permanecer consistentes.

Exemplo:

```text
Confirm Payment
+
Mark Order as Paid
+
Mark Webhook as Processed
```

Não permitir estado parcialmente persistido.

---

# 47. Concorrência

Considerar:

```text
Webhook A
Webhook B
Reconciliation Job
```

processando simultaneamente.

Utilizar quando apropriado:

- unique constraints;
- concurrency tokens;
- transactions;
- idempotent operations.

---

# 48. Reconciliação

Webhooks não serão considerados mecanismo infalível.

Deverá ser possível executar:

```text
Payments AwaitingConfirmation
        ↓
Consultar PSP
        ↓
Comparar estado
        ↓
Reconciliar
```

---

# 49. Background Jobs

Poderão existir para:

- reconciliação;
- expiração de cobranças;
- manutenção.

Ferramenta específica será definida posteriormente.

Não adicionar infraestrutura de jobs sem necessidade concreta.

---

# 50. Domain Events

Poderão representar fatos relevantes:

```text
OrderCreatedDomainEvent
PaymentConfirmedDomainEvent
OrderDeliveredDomainEvent
```

Utilizar apenas quando houver benefício de desacoplamento.

---

# 51. Integration Events

Domain Events e Integration Events são conceitos diferentes.

No MVP:

> não utilizar mensageria distribuída sem necessidade.

Não introduzir antecipadamente:

- RabbitMQ;
- Kafka;
- Azure Service Bus.

---

# 52. Angular

Versão:

> Utilizar versão estável e suportada do Angular definida no início da implementação.

Preferir:

- standalone components;
- lazy loading;
- typed forms;
- signals quando apropriado;
- strict TypeScript;
- interceptors funcionais quando apropriado;
- route guards;
- feature-based organization.

---

# 53. Estrutura Angular por Feature

Zelloa Family:

```text
zelloa-family/
└── src/
    └── app/
        ├── core/
        ├── features/
        │   ├── auth/
        │   ├── students/
        │   ├── catalog/
        │   ├── cart/
        │   ├── orders/
        │   └── payments/
        │
        └── shared/
```

Zelloa School:

```text
zelloa-school/
└── src/
    └── app/
        ├── core/
        ├── features/
        │   ├── dashboard/
        │   ├── cafeteria/
        │   ├── students/
        │   ├── classrooms/
        │   ├── catalog/
        │   ├── orders/
        │   └── settings/
        │
        └── shared/
```

---

# 54. Estado no Frontend

Não introduzir store global complexo antecipadamente.

Utilizar inicialmente:

- Signals;
- services específicos;
- RxJS quando apropriado.

Uma solução global de state management somente deverá ser introduzida se a complexidade justificar.

---

# 55. API Client

Os dois frontends deverão utilizar contratos consistentes com a API.

Preferência por geração de client a partir do OpenAPI quando a solução estiver estabilizada.

Biblioteca:

```text
libs/api-client
```

Evitar manter manualmente modelos duplicados entre backend e frontend quando houver alternativa segura.

---

# 56. Segurança Frontend

Route Guards existem para UX.

Eles não constituem segurança.

A API sempre deverá validar:

- autenticação;
- autorização;
- tenant;
- ownership;
- regras de negócio.

---

# 57. PostgreSQL

Banco:

```text
PostgreSQL
```

Definir consistentemente:

- PK;
- FK;
- indexes;
- unique constraints;
- check constraints;
- precisão financeira;
- timestamps;
- TenantId.

---

# 58. Identificadores

Preferência:

```text
Guid / UUID
```

Evitar identificadores sequenciais públicos quando desnecessários.

---

# 59. Datas e Horários

Backend:

```csharp
DateTimeOffset
```

ou tipos específicos quando semanticamente apropriados.

Instantes deverão ser persistidos consistentemente.

Cada escola deverá possuir timezone.

O limite de pedidos deverá ser configurado por turno no tenant. Como `Classroom.Shift` permanece um texto administrável pela escola, a configuração armazena o mesmo rótulo de turno após trim e comparação sem distinção entre maiúsculas e minúsculas. O pedido deriva o turno da turma atual do aluno, atribui a data operacional usando o dia local da escola e valida o instante com `TimeProvider`. Limite ausente ou já ultrapassado impede a criação.

Evitar:

```csharp
DateTime.Now
```

espalhado pela aplicação.

---

# 60. TimeProvider

Utilizar:

```csharp
TimeProvider
```

para operações dependentes de tempo.

Exemplos:

- horário limite;
- expiração;
- pagamento;
- testes.

---

# 61. Configuração

Segredos nunca deverão ser versionados.

Utilizar:

- environment variables;
- secret stores adequados.

Exemplos:

```text
Database credentials
Authentication secrets
PSP credentials
Webhook secrets
```

---

# 62. Observabilidade

Desde o MVP:

- structured logging;
- correlation ID;
- trace ID;
- health checks;
- métricas essenciais;
- logs de integração externa.

Não registrar:

- senhas;
- tokens completos;
- secrets;
- dados financeiros desnecessários.

---

# 63. OpenTelemetry

A arquitetura deverá ser compatível com:

```text
OpenTelemetry
```

Possíveis instrumentações:

```text
ASP.NET Core
HTTP Client
EF Core
External calls
```

---

# 64. Resiliência

Integrações externas deverão considerar:

- timeout;
- retry seguro;
- circuit breaker quando justificável;
- CancellationToken.

Nunca repetir cegamente operação financeira não idempotente.

---

# 65. OpenAPI

A API deverá disponibilizar documentação OpenAPI.

Será utilizada para:

- documentação;
- frontend;
- testes;
- geração futura do API Client.

---

# 66. Testes

Quatro níveis principais.

## Domain Tests

Testar:

- invariantes;
- Value Objects;
- transições;
- cálculos.

## Application Tests

Testar:

- handlers;
- validações;
- casos de uso.

## Integration Tests

Utilizar PostgreSQL realista.

Preferência:

```text
Testcontainers
```

## Architecture Tests

Validar regras de dependência.

Exemplo:

```text
Domain !→ Infrastructure
Domain !→ Application
Application !→ API
Infrastructure !→ API
```

---

# 67. Testes Frontend

Os frontends deverão possuir testes para comportamentos críticos.

Priorizar:

Zelloa Family:

- carrinho;
- criação do pedido;
- estado de pagamento;
- tratamento de erro.

Zelloa School:

- fila da cantina;
- transições operacionais;
- permissões;
- filtros essenciais.

---

# 68. Testes Financeiros Obrigatórios

Antes do piloto:

```text
✓ pagamento válido
✓ pagamento inexistente
✓ webhook inválido
✓ webhook duplicado
✓ evento fora de ordem
✓ valor divergente
✓ pedido inexistente
✓ pagamento já confirmado
✓ concorrência
✓ timeout do PSP
✓ reconciliação
```

---

# 69. Migrations

Toda alteração estrutural deverá utilizar:

```text
EF Core Migrations
```

Não alterar banco de produção manualmente.

Migrations deverão ser versionadas.

---

# 70. Docker

Ambiente local reproduzível.

Inicialmente:

```text
PostgreSQL
Zelloa API
Zelloa Family
Zelloa School
```

via Docker/Docker Compose quando apropriado.

---

# 71. CI

Pipeline deverá executar pelo menos:

Backend:

```text
Restore
Build
Domain Tests
Application Tests
Architecture Tests
Integration Tests
```

Frontend:

```text
Install
Lint
Test
Type Check
Build Zelloa Family
Build Zelloa School
```

Falhas obrigatórias deverão impedir promoção.

---

# 72. Code Quality

Backend:

```xml
<Nullable>enable</Nullable>
<ImplicitUsings>enable</ImplicitUsings>
<TreatWarningsAsErrors>true</TreatWarningsAsErrors>
```

Frontend:

```text
strict TypeScript
lint
formatting consistente
```

---

# 73. Async

Operações de I/O deverão ser assíncronas.

Utilizar:

```text
CancellationToken
```

Evitar:

```csharp
.Result
.Wait()
```

---

# 74. Dependency Injection

Preferir:

```text
Constructor Injection
```

Evitar:

```text
Service Locator
```

Lifetimes deverão ser definidos corretamente.

---

# 75. SOLID

SOLID orientará decisões sem produzir abstrações artificiais.

Prioridades:

**SRP**

Cada slice representa um caso de uso claro.

**DIP**

Integrações externas dependem de abstrações controladas pela aplicação.

**ISP**

Interfaces pequenas e específicas.

Evitar:

```text
IZelloaService
```

ou serviços gigantescos.

---

# 76. DRY

Não eliminar repetição prematuramente.

> Pequena duplicação é preferível a uma abstração errada.

---

# 77. YAGNI

Não desenvolver antecipadamente:

- microservices;
- event sourcing;
- múltiplos PSPs;
- distributed cache;
- Kubernetes;
- API Gateway;
- mensageria;
- state management complexo;
- Backoffice;
- aplicações nativas.

---

# 78. KISS

Escolher a solução mais simples que preserve:

- corretude;
- segurança;
- clareza;
- manutenção;
- evolução razoável.

---

# 79. Segurança

Aplicar desde o início:

- HTTPS;
- autenticação;
- autorização;
- tenant isolation;
- input validation;
- proteção contra mass assignment;
- secret management;
- webhook validation;
- rate limiting quando necessário;
- logs seguros.

---

# 80. Auditoria

Operações críticas deverão registrar quando apropriado:

```text
TenantId
UserId
Action
Entity
EntityId
Timestamp
CorrelationId
```

---

# 81. Features Iniciais Backend

```text
Application/
│
├── Schools/
│   ├── CreateSchool/
│   └── GetSchool/
│
├── Classrooms/
│   ├── CreateClassroom/
│   └── GetClassrooms/
│
├── Students/
│   ├── CreateStudent/
│   ├── LinkGuardian/
│   └── GetStudents/
│
├── Catalog/
│   ├── CreateCategory/
│   ├── CreateProduct/
│   ├── UpdateProduct/
│   ├── SetProductAvailability/
│   └── GetCatalog/
│
├── Orders/
│   ├── CreateOrder/
│   ├── GetOrder/
│   ├── GetGuardianOrders/
│   ├── CancelOrder/
│   └── RepeatOrder/
│
├── Payments/
│   ├── CreatePixCharge/
│   ├── ProcessWebhook/
│   ├── GetPayment/
│   └── ReconcilePayment/
│
└── Cafeteria/
    ├── GetQueue/
    ├── StartPreparation/
    ├── MarkAsReady/
    └── MarkAsDelivered/
```

---

# 82. Fluxo Principal

```text
ZELLOA FAMILY
      │
      ▼
CreateOrder
      │
      ├── validar responsável
      ├── validar aluno
      ├── validar vínculo
      ├── validar produtos
      ├── calcular preços
      └── criar pedido
              │
              ▼
        CreatePixCharge
              │
              ▼
       Payment Gateway
              │
              ▼
             PSP

             │
         PIX PAGO
             │
             ▼

            PSP
             │
             ▼
       Payment Webhook
             │
             ▼
      ProcessWebhook
             │
             ├── validar autenticidade
             ├── validar idempotência
             ├── validar pagamento
             ├── confirmar Payment
             └── marcar Order como Paid
                         │
                         ▼
                  ZELLOA SCHOOL
                         │
                         ▼
                   Fila da Cantina
                         │
                         ▼
                     Preparing
                         │
                         ▼
                       Ready
                         │
                         ▼
                     Delivered
```

---

# 83. Regra de Ouro do Vertical Slice

Ao desenvolver uma funcionalidade, perguntar:

> Qual caso de uso estamos implementando?

Depois:

> A qual contexto funcional pertence?

Não começar perguntando:

> Em qual Service devemos colocar esse método?

A arquitetura será orientada a casos de uso.

---

# 84. Regras para Agentes de IA

Ao implementar uma feature, o agente deverá:

1. trabalhar apenas no slice solicitado;
2. identificar dependências necessárias;
3. não antecipar funcionalidades;
4. não criar abstrações genéricas preventivamente;
5. não modificar arquitetura compartilhada sem justificativa;
6. manter compatibilidade com decisões anteriores;
7. criar ou atualizar testes;
8. garantir build antes de finalizar;
9. informar alterações realizadas.

O agente não deverá criar sem autorização:

```text
Generic Repository
BaseService
BaseManager
Generic Manager
Event Bus
Microservice
Shared Kernel excessivo
Helpers genéricos preventivos
```

---

# 85. Decisão Arquitetural Final

A arquitetura aprovada para o MVP é:

```text
Backend
.NET 10
ASP.NET Core
Clean Architecture
Vertical Slice Architecture
Modular Monolith
EF Core
PostgreSQL

Frontend Family
Angular
PWA
Mobile-first

Frontend School
Angular
Desktop/Tablet-first

Integrações
REST
Pix / PSP
Webhooks

Infraestrutura
Docker
OpenAPI
OpenTelemetry-ready
Testcontainers
CI/CD
```

---

# 86. Definition of Done da Fundação

A fundação arquitetural será considerada pronta quando:

- solution backend compilar sem warnings;
- dependências respeitarem Clean Architecture;
- Vertical Slice estiver estabelecido;
- PostgreSQL estiver configurado;
- migrations funcionarem;
- autenticação e TenantContext estiverem preparados para evolução, sem exigir sua implementação nesta fase;
- Problem Details estiver configurado;
- validação estiver configurada;
- logging estruturado estiver funcionando;
- health checks estiverem disponíveis;
- OpenAPI estiver funcionando;
- Architecture Tests estiverem passando;
- Integration Tests iniciarem PostgreSQL via container;
- workspace Angular estiver criado;
- Zelloa Family estiver criado;
- Zelloa Family estiver configurado como PWA;
- Zelloa School estiver criado;
- bibliotecas compartilhadas estiverem delimitadas;
- ambos os frontends compilarem;
- ambiente local estiver reproduzível.

Autenticação funcional, identificação do usuário, autorização e implementação do `TenantContext` pertencem à Fase 1, conforme o Plano de Desenvolvimento. A Fase 0 não deverá antecipar esses componentes; sua conclusão exige somente que a fundação permita implementá-los na fase prevista.

---

# 87. Regra de Evolução

Qualquer mudança estrutural significativa deverá ser refletida neste documento antes de ser incorporada ao desenvolvimento.

Decisões futuras não deverão invalidar silenciosamente as decisões arquiteturais existentes.

A arquitetura deverá evoluir de maneira consciente e documentada.

---

**Fim do documento — Zelloa Technical Architecture v1.1**
