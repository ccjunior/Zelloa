# Zelloa — Development Plan

**Documento:** 03-Zelloa-Development-Plan  
**Versão:** 1.0  
**Status:** Plano oficial de desenvolvimento do MVP  
**Produto:** Zelloa  
**Arquitetura:** Clean Architecture + Vertical Slice Architecture  
**Backend:** .NET 10 / ASP.NET Core  
**Frontend Family:** Angular PWA  
**Frontend School:** Angular  
**Banco:** PostgreSQL

---

# 1. Objetivo

Este documento define a ordem oficial de desenvolvimento do MVP da Zelloa.

Seu principal objetivo é permitir desenvolvimento incremental e controlado, inclusive quando executado com auxílio de agentes de IA.

Nenhuma fase deverá ser interpretada como autorização para implementar funcionalidades pertencentes a fases posteriores.

A regra principal é:

> **Implementar → testar → validar → documentar → somente então avançar.**

---

# 2. Fonte de Verdade

O desenvolvimento deverá respeitar, nesta ordem:

1. `01-Zelloa-Product-Specification`
2. `02-Zelloa-Technical-Architecture`
3. `03-Zelloa-Development-Plan`
4. documentação específica da feature em desenvolvimento.

Em caso de conflito entre implementação existente e documentação aprovada, a divergência deverá ser identificada antes de continuar.

Mudanças de escopo ou arquitetura deverão atualizar os documentos correspondentes.

---

# 3. Estratégia Geral

O MVP será desenvolvido incrementalmente.

Ordem:

```text
FASE 0  → Foundation
FASE 1  → Identity + Multi-Tenancy
FASE 2  → School + Academic
FASE 3  → Catalog
FASE 4  → Ordering
FASE 5  → Payments
FASE 6  → Payment Webhooks + Reconciliation
FASE 7  → Cafeteria Operations
FASE 8  → Zelloa Family
FASE 9  → Zelloa School
FASE 10 → Dashboard + Audit
FASE 11 → Hardening + Security
FASE 12 → Pilot Deployment
```

Cada fase possui um **Gate**.

Nenhuma fase é considerada concluída enquanto o Gate não for atendido.

---

# 4. Regras Gerais de Execução

Antes de iniciar qualquer fase, o agente deverá:

1. ler os documentos 01, 02 e 03;
2. identificar a fase atual;
3. verificar as dependências;
4. analisar o código existente;
5. listar alterações planejadas;
6. somente então iniciar implementação.

Ao finalizar:

1. executar build;
2. executar testes aplicáveis;
3. verificar warnings;
4. verificar migrations quando aplicável;
5. verificar alterações arquiteturais;
6. apresentar resumo;
7. aguardar autorização para próxima fase.

---

# 5. Proibição de Desenvolvimento Antecipado

O agente não deverá implementar uma feature porque:

- será necessária futuramente;
- parece simples;
- seria conveniente;
- poderia economizar tempo depois.

Exemplo:

Durante `Catalog`, não implementar carrinho.

Durante `Ordering`, não implementar webhook Pix.

Durante `Payments`, não implementar dashboard.

Durante `Family`, não implementar funcionalidades administrativas.

---

# FASE 0 — FOUNDATION

# 6. Objetivo

Construir a fundação técnica da Zelloa sem implementar regras funcionais do produto.

---

# 7. Backend Foundation

Criar:

```text
src/
├── Zelloa.Domain/
├── Zelloa.Application/
├── Zelloa.Infrastructure/
└── Zelloa.Api/
```

Testes:

```text
tests/
├── Zelloa.Domain.Tests/
├── Zelloa.Application.Tests/
├── Zelloa.IntegrationTests/
└── Zelloa.ArchitectureTests/
```

Configurar:

- .NET 10;
- nullable reference types;
- warnings as errors;
- dependency injection;
- configuração por ambiente;
- Problem Details;
- exception handling;
- health checks;
- OpenAPI;
- structured logging;
- TimeProvider;
- EF Core;
- PostgreSQL.

---

# 8. Architecture Tests

Criar testes garantindo:

```text
Domain !→ Application
Domain !→ Infrastructure
Domain !→ Api

Application !→ Infrastructure
Application !→ Api

Infrastructure !→ Api
```

Dependências permitidas deverão respeitar o documento 02.

---

# 9. PostgreSQL

Criar infraestrutura inicial de banco.

Configurar:

```text
ZelloaDbContext
```

Ainda não criar tabelas especulativas.

Criar apenas infraestrutura necessária para suportar as fases seguintes.

---

# 10. Frontend Workspace

Criar workspace Angular.

Aplicações:

```text
zelloa-family
zelloa-school
```

Bibliotecas iniciais:

```text
api-client
authentication
shared-ui
utilities
```

Somente criar estrutura.

Não implementar funcionalidades de negócio.

---

# 11. Zelloa Family Foundation

Configurar:

- Angular;
- strict mode;
- routing;
- environments;
- HTTP client;
- PWA;
- manifest;
- service worker;
- layout base.

Nenhum fluxo de compra deverá existir nesta fase.

---

# 12. Zelloa School Foundation

Configurar:

- Angular;
- strict mode;
- routing;
- environments;
- HTTP client;
- layout base.

Não implementar administração ainda.

---

# 13. Docker Local

Criar ambiente local contendo inicialmente:

```text
PostgreSQL
Zelloa API
Zelloa Family
Zelloa School
```

O ambiente deverá possuir documentação para inicialização.

---

# 14. CI Inicial

Pipeline deverá executar:

Backend:

```text
Restore
Build
Architecture Tests
```

Frontend:

```text
Install
Lint
Test
Build Family
Build School
```

---

# 15. Gate — Fase 0

Somente concluir quando:

- backend compilar;
- zero warnings;
- Architecture Tests passarem;
- PostgreSQL estiver acessível;
- API iniciar;
- health check responder;
- OpenAPI funcionar;
- Family compilar;
- Family possuir PWA configurado;
- School compilar;
- Docker local funcionar;
- CI inicial estiver funcionando.

---

# FASE 1 — IDENTITY E MULTI-TENANCY

# 16. Objetivo

Criar identidade, autenticação, autorização e isolamento institucional.

Essa fase deverá ser concluída antes de dados funcionais multi-tenant.

---

# 17. Identity

Implementar perfis:

```text
PlatformAdmin
SchoolAdmin
CafeteriaOperator
Guardian
```

Implementar autenticação conforme estratégia definida durante a fase.

Não criar mecanismo próprio de criptografia.

Decisão aprovada em 2026-10-06: utilizar ASP.NET Core Identity com sessão por cookie autenticado; não emitir JWT próprio nem integrar provedor OIDC externo nesta fase. Proteger endpoints que alteram estado contra CSRF e configurar CORS com origins explícitas.

Decisão aprovada em 2026-10-06: cada conta institucional pertence a uma instituição, e uma instituição pode ter várias contas institucionais. Isso permite contas distintas para vários responsáveis da mesma escola. `PlatformAdmin` é global e não pertence a tenant.

---

# 18. Current User

Criar abstração para usuário autenticado.

Exemplo conceitual:

```text
ICurrentUser

UserId
IsAuthenticated
Roles
```

Expor o usuário autenticado por `GET /api/me`. O endpoint deverá informar `tenantId` para contas institucionais e `null` para `PlatformAdmin`.

---

# 19. Tenant Context

Criar:

```text
ITenantContext
```

O tenant deverá ser resolvido a partir de contexto confiável.

Não aceitar `TenantId` arbitrário do frontend como fonte de autorização.

---

# 20. Tenant

Criar entidade e persistência mínima para:

```text
Tenant
```

Campos definitivos deverão seguir necessidade atual.

Evitar informações especulativas.

---

# 21. Autorização

Criar policies iniciais.

Exemplos:

```text
CanManageSchool
CanManageCatalog
CanOperateCafeteria
CanCreateOrder
```

---

# 22. Testes de Segurança

Criar testes demonstrando:

- usuário não autenticado é rejeitado;
- usuário sem policy é rejeitado;
- usuário de Tenant A não acessa Tenant B;
- TenantId enviado pelo cliente não sobrescreve contexto autenticado.

---

# 23. Gate — Fase 1

Concluir somente quando:

- autenticação funcionar;
- usuário atual puder ser identificado;
- TenantContext funcionar;
- policies funcionarem;
- isolamento possuir testes;
- migrations estiverem válidas;
- testes passarem;
- build sem warnings.

---

# FASE 2 — SCHOOL E ACADEMIC

# 24. Objetivo

Implementar estrutura mínima necessária para representar escola, turmas, alunos, responsáveis e vínculos.

---

# 25. School

Slices iniciais:

```text
CreateSchool
GetSchool
UpdateSchool
InviteSchoolAdmin
ActivateInvitedAccount
```

Informações apenas necessárias ao MVP.

---

# 26. Classroom

Slices:

```text
CreateClassroom
UpdateClassroom
GetClassrooms
SetClassroomStatus
```

Dados:

- nome;
- série/nível;
- turno;
- ano letivo;
- status.

---

# 27. Student

Slices:

```text
CreateStudent
UpdateStudent
GetStudent
GetStudents
SetStudentStatus
```

Aplicar minimização de dados.

---

# 28. Guardian

Implementar responsável e vínculo.

Slices:

```text
CreateGuardian
GetGuardian
LinkGuardianToStudent
UnlinkGuardianFromStudent
GetGuardianStudents
```

`CreateSchool` será executado por `PlatformAdmin` e criará o Tenant correspondente. `InviteSchoolAdmin` permite ao `PlatformAdmin` gerar o convite do administrador inicial. `CreateGuardian` será executado por `SchoolAdmin`, criará a conta pendente e devolverá um link de ativação de uso único para entrega manual. O usuário define sua senha ao ativar o convite. Convites expiram em 24 horas; não integrar provedor de envio nesta fase.

---

# 29. Ownership

Garantir:

> Guardian somente poderá acessar alunos explicitamente vinculados.

Criar testes específicos.

---

# 30. Gate — Fase 2

Deverá ser possível:

1. criar escola;
2. criar turma;
3. criar aluno;
4. criar responsável;
5. vincular responsável;
6. consultar alunos do responsável;
7. impedir acesso indevido;
8. respeitar tenant.
9. provisionar o administrador escolar inicial e ativar sua conta por convite;
10. ativar a conta de um responsável convidado sem conceder acesso a aluno até o vínculo explícito;
11. rejeitar token expirado ou reutilizado.

Build e testes obrigatoriamente passando.

---

# FASE 3 — CATALOG

# 31. Objetivo

Criar o catálogo de alimentação escolar.

---

# 32. Category

Slices:

```text
CreateCategory
UpdateCategory
GetCategories
SetCategoryStatus
```

---

# 33. Product

Slices:

```text
CreateProduct
UpdateProduct
GetProduct
GetProducts
SetProductAvailability
SetProductStatus
```

Dados mínimos:

- nome;
- descrição opcional;
- categoria;
- preço;
- imagem opcional;
- status;
- disponibilidade.

---

# 34. Regras

Garantir:

- preço válido;
- categoria pertencente ao tenant;
- produto pertencente ao tenant;
- produto inativo não disponível para novos pedidos;
- produto indisponível não comprável.

---

# 35. Consulta de Cardápio

Criar query otimizada para experiência Family.

Exemplo:

```text
GetAvailableCatalog
```

Retornar somente dados necessários.

---

# 36. Gate — Fase 3

Deverá ser possível:

- cadastrar categorias;
- cadastrar produtos;
- alterar preços;
- alterar disponibilidade;
- consultar cardápio disponível;
- garantir isolamento por tenant.

Testes obrigatórios.

---

# FASE 4 — ORDERING

# 37. Objetivo

Implementar criação e gerenciamento de pedidos sem integração financeira real ainda.

---

# 38. Order

Criar Aggregate Root:

```text
Order
```

Itens:

```text
OrderItem
```

---

# 39. Snapshot

OrderItem deverá preservar:

- ProductId;
- ProductName;
- UnitPrice;
- Quantity;
- Subtotal.

Alterações futuras do produto não alteram pedido existente.

---

# 40. CreateOrder

Fluxo:

```text
Guardian
   ↓
Student
   ↓
Validar vínculo
   ↓
Produtos
   ↓
Validar disponibilidade
   ↓
Calcular valores
   ↓
Criar Order
```

O backend recalcula tudo.

Nunca confiar em preço enviado pelo frontend.

---

# 41. Estados Iniciais

Implementar:

```text
Created
AwaitingPayment
Paid
Preparing
Ready
Delivered
Cancelled
PaymentExpired
```

Somente transições necessárias nesta fase deverão ser utilizadas.

---

# 42. Cancelamento

Implementar inicialmente cancelamento de pedido não pago.

Pedido pago não deverá ser automaticamente estornado.

---

# 43. RepeatOrder

Implementar:

```text
RepeatOrder
```

Revalidando:

- produtos;
- preços;
- disponibilidade;
- regras;
- horário.

Nunca copiar cegamente valores históricos.

---

# 44. Horário Limite

Implementar limite de pedido configurável pelo SchoolAdmin para cada turno, sem data de entrega informada pelo responsável. O turno é derivado da turma atual do aluno; a data operacional é o dia local corrente da escola. Rejeitar a criação se o turno não possuir limite configurado ou se o horário local tiver passado do limite.

Utilizar:

```text
TimeProvider
```

e timezone institucional.

---

# 45. Testes Ordering

Obrigatórios:

- preço adulterado no cliente;
- produto inexistente;
- produto indisponível;
- produto inativo;
- aluno não vinculado;
- tenant incorreto;
- quantidade inválida;
- cutoff expirado;
- cutoff não configurado;
- snapshot;
- repetição com preço alterado.

---

# 46. Gate — Fase 4

Deverá ser possível criar pedido válido sem pagamento real.

Toda regra financeira do pedido deverá estar protegida pelo backend.

---

# FASE 5 — PAYMENTS

# 47. Objetivo

Integrar um PSP real e gerar cobranças Pix.

---

# 48. Seleção do PSP

Antes de codificar:

1. avaliar providers;
2. verificar API Pix;
3. verificar webhook;
4. verificar sandbox;
5. verificar tarifas;
6. verificar modelo de recebimento;
7. selecionar um provider.

Registrar decisão técnica.

---

# 49. Payment Domain

Criar:

```text
Payment
```

com ciclo de vida próprio.

Não utilizar Order como registro financeiro.

---

# 50. Payment Gateway

Implementar abstração:

```text
IPaymentGateway
```

e implementação específica na Infrastructure.

---

# 51. CreatePixCharge

Fluxo:

```text
Order
 ↓
Payment
 ↓
PSP
 ↓
Pix Charge
 ↓
QR Code
Pix Copy/Paste
Expiration
```

---

# 52. Persistência

Registrar no mínimo:

- PaymentId;
- OrderId;
- provider;
- external transaction ID;
- valor esperado;
- status;
- criação;
- expiração;
- confirmação quando aplicável.

---

# 53. Segurança

Credenciais:

- fora do código;
- fora do Git;
- via secrets/environment.

Logs não poderão conter secrets.

---

# 54. Gate — Fase 5

Deverá ser possível:

1. criar pedido;
2. criar Payment;
3. gerar cobrança real no ambiente de homologação/sandbox;
4. receber QR Code;
5. receber Pix Copia e Cola;
6. consultar estado financeiro.

Ainda não considerar webhook concluído.

---

# FASE 6 — WEBHOOK E RECONCILIAÇÃO

# 55. Objetivo

Automatizar confirmação financeira de maneira segura e idempotente.

---

# 56. ProcessPaymentWebhook

Criar slice específico.

Fluxo:

```text
Receive
 ↓
Authenticate
 ↓
Parse
 ↓
Identify
 ↓
Idempotency
 ↓
Validate
 ↓
Payment
 ↓
Order
 ↓
Commit
```

---

# 57. Webhook Event

Persistir eventos necessários para idempotência.

Unique constraint no identificador apropriado.

---

# 58. Confirmação

Somente confirmação financeira válida poderá provocar:

```text
Payment → Paid
Order → Paid
```

Frontend nunca executa essa transição diretamente.

---

# 59. Atomicidade

Persistir atomicamente quando necessário:

```text
Payment confirmed
Order paid
Webhook processed
```

---

# 60. Reconciliation

Criar processo capaz de consultar PSP para pagamentos pendentes.

Objetivo:

> recuperar inconsistências causadas por webhook perdido ou falha temporária.

---

# 61. Testes Obrigatórios

```text
valid webhook
invalid webhook
duplicate webhook
wrong amount
unknown payment
already paid
concurrent processing
out-of-order event
PSP unavailable
reconciliation success
```

---

# 62. Gate — Fase 6

Fluxo real deverá funcionar:

```text
Order
→ Pix
→ Payment
→ Webhook
→ Confirmation
→ Order Paid
```

sem intervenção manual.

---

# FASE 7 — CAFETERIA OPERATIONS

# 63. Objetivo

Transformar pedidos pagos em fila operacional.

---

# 64. GetCafeteriaQueue

A fila deverá exibir apenas pedidos elegíveis para operação.

Não tratar `AwaitingPayment` como pedido para preparação.

---

# 65. Transições

Implementar:

```text
Paid
 ↓
Preparing
 ↓
Ready
 ↓
Delivered
```

---

# 66. Dados Operacionais

Apresentar:

- aluno;
- turma;
- produtos;
- quantidades;
- horário;
- status.

---

# 67. Auditoria de Entrega

Ao marcar como entregue:

registrar:

```text
DeliveredAt
DeliveredBy
```

---

# 68. Concorrência Operacional

Dois operadores não deverão conseguir provocar transições inconsistentes simultaneamente.

---

# 69. Gate — Fase 7

Deverá ser possível executar:

```text
Pedido pago
→ visualizar
→ preparar
→ pronto
→ entregar
```

com rastreabilidade.

---

# FASE 8 — ZELLOA FAMILY

# 70. Objetivo

Construir a experiência completa do responsável.

---

# 71. Autenticação

Implementar experiência de autenticação do Guardian.

---

# 72. Home

Após login:

- identificar responsável;
- apresentar alunos vinculados;
- permitir seleção.

---

# 73. Catálogo

Tela mobile-first.

Permitir:

- categorias;
- produtos;
- preços;
- disponibilidade.

---

# 74. Carrinho

Implementar:

- adicionar;
- remover;
- quantidade;
- subtotal visual;
- total estimado.

Importante:

> valor exibido no frontend não substitui cálculo do backend.

---

# 75. Checkout

Fluxo:

```text
Aluno
 ↓
Carrinho
 ↓
Revisão
 ↓
Criar pedido
 ↓
Pix
```

---

# 76. Pix

Exibir:

- QR Code;
- Pix Copia e Cola;
- total;
- expiração;
- estado.

---

# 77. Acompanhamento

Após pagamento:

```text
Pagamento confirmado
Preparando
Pronto
Entregue
```

Implementação de atualização poderá utilizar inicialmente polling controlado.

Não introduzir WebSocket sem necessidade.

---

# 78. Histórico

Responsável poderá consultar pedidos anteriores.

---

# 79. Repetir Pedido

Disponibilizar UX para utilizar o slice já existente.

Sempre exibir condições atuais antes da confirmação.

---

# 80. PWA

Validar:

- manifest;
- ícones;
- instalação;
- atualização;
- service worker;
- comportamento de cache;
- experiência mobile.

Dados financeiros não deverão ser tratados como confiáveis quando offline.

---

# 81. Gate — Fase 8

Um responsável deverá conseguir realizar toda a jornada pelo celular:

```text
Login
→ Aluno
→ Cardápio
→ Carrinho
→ Pedido
→ Pix
→ Confirmação
→ Acompanhamento
→ Histórico
```

---

# FASE 9 — ZELLOA SCHOOL

# 82. Objetivo

Construir interface operacional da escola.

---

# 83. Autenticação

SchoolAdmin e CafeteriaOperator.

Menus deverão respeitar autorização.

---

# 84. Administração Acadêmica

SchoolAdmin poderá administrar:

- turmas;
- alunos;
- responsáveis;
- vínculos.

---

# 85. Catálogo

SchoolAdmin poderá:

- criar categoria;
- criar produto;
- editar;
- alterar preço;
- ativar/desativar;
- disponibilizar/indisponibilizar.

---

# 86. Cantina

CafeteriaOperator deverá possuir tela operacional otimizada para:

```text
Paid
Preparing
Ready
Delivered
```

---

# 87. Pedidos

Permitir consulta e filtros necessários.

---

# 88. Gate — Fase 9

A escola deverá conseguir operar o fluxo sem acessar banco, Swagger ou ferramentas técnicas.

---

# FASE 10 — DASHBOARD E AUDITORIA

# 89. Objetivo

Adicionar visibilidade operacional mínima.

---

# 90. Dashboard

Indicadores:

- pedidos do dia;
- pagos;
- aguardando pagamento;
- preparando;
- prontos;
- entregues;
- cancelados;
- valor confirmado.

---

# 91. Auditoria

Operações críticas deverão possuir trilha adequada.

Priorizar:

- pagamento;
- status de pedido;
- entrega;
- alterações administrativas relevantes.

---

# 92. Gate — Fase 10

Administração deverá conseguir acompanhar operação básica e investigar eventos relevantes.

---

# FASE 11 — HARDENING E SECURITY

# 93. Objetivo

Preparar aplicação para piloto real.

---

# 94. Revisão de Segurança

Validar:

- authentication;
- authorization;
- tenant isolation;
- ownership;
- rate limiting;
- secrets;
- webhook;
- CORS;
- headers;
- logs;
- input validation.

---

# 95. LGPD

Revisar:

- dados armazenados;
- necessidade;
- exposição;
- logs;
- retenção;
- permissões.

Remover dados não necessários.

---

# 96. Performance

Executar testes nos fluxos críticos.

Não otimizar prematuramente.

Investigar apenas gargalos medidos.

---

# 97. Observabilidade

Garantir:

- structured logs;
- trace/correlation ID;
- health checks;
- erros externos rastreáveis;
- métricas essenciais.

---

# 98. Teste Ponta a Ponta

Executar cenário:

```text
Admin cria turma
→ cria aluno
→ vincula responsável
→ cria produto

Guardian entra
→ seleciona aluno
→ cria pedido
→ paga Pix

PSP confirma
→ webhook processado

Cantina recebe
→ prepara
→ marca pronto
→ entrega

Guardian consulta histórico
```

---

# 99. Gate — Fase 11

Nenhuma vulnerabilidade ou falha funcional crítica conhecida poderá permanecer aberta para início do piloto.

---

# FASE 12 — PILOT DEPLOYMENT

# 100. Objetivo

Publicar ambiente utilizável por uma instituição piloto.

---

# 101. Ambientes

Manter separação mínima:

```text
Development
Staging
Production
```

---

# 102. Deployment

Publicar:

```text
Zelloa API
Zelloa Family
Zelloa School
PostgreSQL
```

Configurar:

- HTTPS;
- DNS;
- secrets;
- backups;
- migrations;
- logs;
- health checks.

---

# 103. Domínios

Sugestão conceitual:

```text
app.zelloa.com.br
school.zelloa.com.br
api.zelloa.com.br
```

A configuração definitiva será tomada durante deployment.

---

# 104. Banco

Produção deverá possuir:

- backup;
- política de retenção;
- acesso restrito;
- credenciais exclusivas;
- conexão segura.

---

# 105. PSP Produção

Antes de ativar:

- credenciais corretas;
- webhook de produção;
- HTTPS;
- validação;
- teste controlado de pagamento;
- reconciliação.

---

# 106. Smoke Test

Após deployment executar:

```text
Login
Catalog
Order
Pix
Payment
Webhook
Cafeteria
Delivery
History
```

---

# 107. Gate — Fase 12

Piloto somente inicia após:

- smoke tests;
- backup validado;
- pagamento validado;
- webhook validado;
- HTTPS;
- observabilidade;
- acesso da escola;
- acesso de responsáveis de teste.

---

# 108. Fluxo Completo das Fases

```text
┌──────────────────────────┐
│ 0. Foundation            │
└────────────┬─────────────┘
             ▼
┌──────────────────────────┐
│ 1. Identity / Tenant     │
└────────────┬─────────────┘
             ▼
┌──────────────────────────┐
│ 2. School / Academic     │
└────────────┬─────────────┘
             ▼
┌──────────────────────────┐
│ 3. Catalog               │
└────────────┬─────────────┘
             ▼
┌──────────────────────────┐
│ 4. Ordering              │
└────────────┬─────────────┘
             ▼
┌──────────────────────────┐
│ 5. Payments              │
└────────────┬─────────────┘
             ▼
┌──────────────────────────┐
│ 6. Webhook/Reconciliation│
└────────────┬─────────────┘
             ▼
┌──────────────────────────┐
│ 7. Cafeteria             │
└────────────┬─────────────┘
             ▼
┌──────────────────────────┐
│ 8. Zelloa Family         │
└────────────┬─────────────┘
             ▼
┌──────────────────────────┐
│ 9. Zelloa School         │
└────────────┬─────────────┘
             ▼
┌──────────────────────────┐
│ 10. Dashboard/Audit      │
└────────────┬─────────────┘
             ▼
┌──────────────────────────┐
│ 11. Hardening            │
└────────────┬─────────────┘
             ▼
┌──────────────────────────┐
│ 12. Pilot                │
└──────────────────────────┘
```

---

# 109. Regra de Checkpoint

Após cada fase, o agente deverá apresentar:

```text
FASE CONCLUÍDA: X

Implementado:
- ...

Arquivos principais:
- ...

Migrations:
- ...

Testes adicionados:
- ...

Resultado dos testes:
- ...

Resultado do build:
- ...

Pendências:
- ...

Decisões tomadas:
- ...

Alterações na arquitetura:
- nenhuma / descrição

Pronto para próxima fase:
SIM / NÃO
```

O agente não deverá iniciar automaticamente a próxima fase.

---

# 110. Regra para Código Existente

Antes de criar uma implementação, verificar se já existe solução equivalente.

Evitar:

- classes duplicadas;
- DTOs duplicados;
- validators duplicados;
- abstrações concorrentes;
- segundo padrão para o mesmo problema.

---

# 111. Regra para Refactoring

Refactoring é permitido quando necessário para implementar corretamente a fase atual.

Entretanto, alterações amplas deverão ser justificadas antes da execução.

Não realizar "cleanup geral" durante implementação de uma feature específica.

---

# 112. Regra para TODOs

Não utilizar TODO como mecanismo para considerar uma funcionalidade concluída.

Se algo necessário para o Gate não foi implementado:

> A fase permanece incompleta.

TODOs para funcionalidades futuras poderão existir apenas quando claramente fora do escopo atual.

---

# 113. Regra para Testes

Não remover ou desabilitar testes apenas para permitir que pipeline passe.

Não utilizar:

```text
Skip
Ignore
Comment out
```

para esconder regressões sem justificativa explícita.

---

# 114. Regra para Migrations

Antes de concluir fase com alteração de persistência:

- migration criada;
- migration revisada;
- banco limpo consegue aplicar migrations;
- Integration Tests passam.

---

# 115. Regra para Dependências

Antes de adicionar pacote externo:

1. justificar necessidade;
2. verificar se plataforma já resolve;
3. avaliar manutenção;
4. evitar pacote para problema trivial.

---

# 116. Regra para Segurança

Nunca:

- confiar em dados financeiros do frontend;
- confiar em TenantId enviado pelo cliente;
- considerar QR Code como pagamento;
- considerar comprovante como pagamento;
- registrar secrets;
- expor stack trace;
- permitir mudança arbitrária de status.

---

# 117. Regra para Pagamentos

Toda alteração financeira deverá ser:

- rastreável;
- idempotente quando necessário;
- validada;
- protegida contra concorrência;
- associada ao tenant correto;
- associada ao pedido correto.

---

# 118. Regra para PWA

O PWA deverá melhorar a experiência.

Nunca permitir que comportamento offline comprometa consistência financeira.

Em dúvida:

> consultar a API.

---

# 119. Regra de Simplicidade

Quando existirem duas soluções tecnicamente corretas, preferir a que:

1. possuir menos componentes;
2. possuir menor acoplamento;
3. for mais simples de testar;
4. for mais simples de operar;
5. atender integralmente o requisito atual.

---

# 120. Definition of Done Global

O MVP da Zelloa será considerado concluído quando:

- todas as fases obrigatórias estiverem concluídas;
- builds estiverem passando;
- testes estiverem passando;
- migrations estiverem consistentes;
- integração Pix estiver funcional;
- webhook estiver seguro e idempotente;
- multi-tenancy estiver validado;
- Guardian possuir isolamento correto;
- Zelloa Family funcionar como PWA;
- Zelloa School permitir operação completa;
- fluxo da cantina estiver funcional;
- observabilidade mínima existir;
- ambiente de produção estiver publicado;
- fluxo ponta a ponta estiver validado.

---

# 121. Princípio Final

A prioridade da Zelloa não é quantidade de funcionalidades.

A prioridade é entregar um fluxo pequeno, seguro e completo:

> **Escolher → Pedir → Pagar → Confirmar → Preparar → Entregar**

Somente depois que esse fluxo estiver sólido novas funcionalidades deverão ser consideradas.

---

**Fim do documento — Zelloa Development Plan v1.0**
