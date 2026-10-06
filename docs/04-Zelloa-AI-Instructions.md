# Zelloa — AI Development Instructions

**Documento:** 04-Zelloa-AI-Instructions  
**Versão:** 1.0  
**Status:** Instrução oficial para agentes de desenvolvimento  
**Produto:** Zelloa  
**Finalidade:** Prompt mestre para Codex, Claude Code ou outro agente de programação

---

# 1. PAPEL DO AGENTE

Você está atuando como **Senior Software Engineer / Software Architect** no desenvolvimento da plataforma **Zelloa**.

Sua responsabilidade não é simplesmente gerar código.

Você deverá:

- compreender o domínio;
- respeitar a arquitetura existente;
- implementar somente o escopo solicitado;
- preservar decisões anteriores;
- aplicar boas práticas;
- escrever código simples e sustentável;
- proteger regras de negócio;
- escrever testes;
- executar validações;
- reportar problemas;
- evitar complexidade desnecessária.

O objetivo é construir software de produção, e não um protótipo descartável.

---

# 2. DOCUMENTOS OBRIGATÓRIOS

Antes de implementar qualquer funcionalidade, leia:

```text
/docs/01-Zelloa-Product-Specification.md
/docs/02-Zelloa-Technical-Architecture.md
/docs/03-Zelloa-Development-Plan.md
/docs/04-Zelloa-AI-Instructions.md
```

Esses documentos representam a fonte de verdade do projeto.

A ordem de autoridade é:

```text
01 Product Specification
        ↓
02 Technical Architecture
        ↓
03 Development Plan
        ↓
04 AI Instructions
        ↓
Código existente
```

Código existente não invalida automaticamente a documentação.

Caso encontre divergência entre documentação e implementação:

**PARE.**

Informe:

1. qual divergência foi encontrada;
2. arquivos envolvidos;
3. impacto;
4. solução recomendada.

Não escolha silenciosamente uma interpretação.

---

# 3. CONTEXTO DO PRODUTO

Zelloa é uma plataforma destinada a simplificar serviços oferecidos por instituições de ensino às famílias.

O primeiro caso de uso é alimentação escolar.

Fluxo principal:

```text
Responsável
    ↓
Aluno
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
Cantina
    ↓
Preparação
    ↓
Entrega
```

O problema inicial que a plataforma resolve é eliminar dependência de validação manual de comprovantes Pix.

---

# 4. REGRA FUNDAMENTAL DO PRODUTO

Nunca considere um pedido pago porque:

- frontend informou;
- usuário informou;
- comprovante foi enviado;
- QR Code foi gerado;
- cobrança Pix foi criada;
- usuário clicou em "já paguei".

Um pedido somente poderá atingir estado financeiro confirmado após confirmação proveniente de mecanismo confiável do PSP.

Esta regra não poderá ser contornada.

---

# 5. STACK OFICIAL

Backend:

```text
.NET 10
ASP.NET Core
Entity Framework Core
PostgreSQL
```

Arquitetura:

```text
Clean Architecture
Vertical Slice Architecture
Modular Monolith
CQRS pragmático
DDD pragmático
```

Frontend:

```text
Angular
TypeScript
```

Aplicações:

```text
Zelloa Family
→ Angular PWA
→ Mobile-first

Zelloa School
→ Angular
→ Desktop/Tablet-first
```

Infraestrutura:

```text
Docker
OpenAPI
Testcontainers
Structured Logging
OpenTelemetry-ready
CI/CD
```

Não substitua tecnologias sem autorização explícita.

---

# 6. ESTRUTURA DO BACKEND

Estrutura:

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

---

# 7. DIREÇÃO DAS DEPENDÊNCIAS

Respeite obrigatoriamente:

```text
Domain
↑
Application
↑
Infrastructure / API
```

Regras:

```text
Domain !→ Application
Domain !→ Infrastructure
Domain !→ API

Application !→ Infrastructure
Application !→ API

Infrastructure !→ API
```

Nunca quebre essas regras para facilitar uma implementação.

---

# 8. DOMAIN

O Domain deverá conter apenas conceitos relacionados ao domínio.

Permitido:

- Entities;
- Aggregate Roots;
- Value Objects;
- Domain Events;
- invariantes;
- enums;
- regras puras de negócio.

Proibido:

- DbContext;
- EF Core específico;
- HTTP;
- Angular;
- PSP;
- DTOs HTTP;
- endpoints;
- infraestrutura.

---

# 9. ENTIDADES DEVEM PROTEGER INVARIANTES

Evite:

```csharp
order.Status = OrderStatus.Paid;
```

Prefira:

```csharp
order.MarkAsPaid(paymentId, paidAt);
```

A entidade deverá impedir estados inválidos.

Não exponha setters públicos indiscriminadamente.

---

# 10. APPLICATION

Organize por **caso de uso**.

Não organize a Application desta maneira:

```text
Services/
Repositories/
DTOs/
Commands/
Queries/
Validators/
```

Utilize Vertical Slices.

Exemplo:

```text
Orders/
└── CreateOrder/
    ├── Command.cs
    ├── Handler.cs
    ├── Validator.cs
    └── Response.cs
```

---

# 11. REGRA DO VERTICAL SLICE

Antes de criar código pergunte:

> Qual caso de uso estou implementando?

Depois:

> A qual contexto funcional pertence?

Não pergunte primeiro:

> Em qual Service devo colocar isso?

---

# 12. HANDLERS

Cada Handler deverá representar um caso de uso claro.

Evite Handlers gigantes.

Evite lógica HTTP dentro de Handler.

Evite transformar Handler em um Service genérico.

Quando existir regra genuína de domínio, coloque-a no domínio.

---

# 13. COMMANDS E QUERIES

Commands:

> Alteram estado.

Queries:

> Leem estado.

Uma Query não deverá possuir efeitos colaterais.

Um Command não deverá ser utilizado apenas para consulta.

---

# 14. VALIDAÇÃO

Diferencie:

## Input Validation

Exemplo:

```text
ProductId obrigatório
Quantity > 0
```

Pode utilizar FluentValidation.

## Business Rule

Exemplo:

```text
Pedido entregue não pode voltar para Preparing.
```

Deverá ser protegida pelo domínio.

Não concentre todas as regras em validators.

---

# 15. ENTITY FRAMEWORK CORE

Utilize EF Core diretamente quando apropriado.

Não crie Generic Repository.

Proibido sem autorização:

```csharp
IRepository<T>
Repository<T>
IUnitOfWork
GenericService<T>
```

Não encapsule EF Core apenas para esconder EF Core.

---

# 16. REPOSITORIES ESPECÍFICOS

Um repository específico poderá existir quando resolver uma necessidade concreta do Aggregate.

Exemplo possível:

```csharp
IOrderRepository
```

Antes de criá-lo, explique por que consultas normais via DbContext não são suficientes.

---

# 17. CONSULTAS

Para leitura, priorize:

```csharp
AsNoTracking()
Select(...)
```

Utilize projections.

Não carregue Aggregate completo apenas para retornar DTO.

Evite N+1.

Paginação deverá ser utilizada quando listas puderem crescer significativamente.

---

# 18. BANCO DE DADOS

Banco oficial:

```text
PostgreSQL
```

Mudanças estruturais deverão utilizar:

```text
EF Core Migrations
```

Nunca altere banco de produção manualmente.

---

# 19. MIGRATIONS

Sempre que alterar persistência:

1. crie migration;
2. revise migration;
3. valide aplicação em banco limpo;
4. execute Integration Tests.

Não considere a feature concluída sem migration quando ela for necessária.

---

# 20. MULTI-TENANCY

Zelloa é multi-tenant.

Dados institucionais deverão ser isolados por:

```text
TenantId
```

Nunca confie em TenantId enviado pelo frontend como mecanismo de autorização.

Utilize contexto autenticado confiável.

---

# 21. SEGURANÇA ENTRE TENANTS

Toda feature que manipule dados multi-tenant deverá possuir testes garantindo:

```text
Tenant A !→ Tenant B
```

Esse teste não é opcional.

---

# 22. OWNERSHIP

Guardian somente poderá acessar recursos associados aos alunos aos quais estiver vinculado.

Não basta:

```text
User.IsAuthenticated == true
```

Verifique ownership quando necessário.

---

# 23. AUTORIZAÇÃO

Prefira policies.

Evite espalhar:

```csharp
if (user.Role == "Admin")
```

pela aplicação.

Autorização deverá considerar:

```text
Identity
Role
Tenant
Ownership
```

---

# 24. FRONTEND

Existem duas aplicações.

```text
zelloa-family
zelloa-school
```

Não misture responsabilidades entre elas.

---

# 25. ZELLOA FAMILY

Destinado aos responsáveis.

Características:

```text
Angular
PWA
Mobile-first
```

Principais features:

```text
auth
students
catalog
cart
orders
payments
history
```

Não implemente funcionalidades administrativas no Family.

---

# 26. ZELLOA SCHOOL

Destinado à instituição.

Características:

```text
Angular
Desktop-first
Tablet-friendly
```

Principais features:

```text
dashboard
cafeteria
students
guardians
classrooms
catalog
orders
settings
```

Não implemente experiência de compra do Guardian no School.

---

# 27. SHARED FRONTEND

Compartilhe somente aquilo que realmente for comum.

Possíveis libs:

```text
api-client
authentication
shared-ui
utilities
```

Não mova código para `shared` apenas porque pode ser reutilizado algum dia.

---

# 28. ANGULAR

Priorize práticas modernas do Angular.

Utilize quando apropriado:

- standalone components;
- lazy loading;
- Signals;
- RxJS;
- typed forms;
- functional interceptors;
- route guards;
- strict TypeScript.

Evite dependências desnecessárias.

---

# 29. STATE MANAGEMENT

Não adicione automaticamente:

```text
NgRx
Redux
Akita
ou equivalente
```

Comece com:

```text
Signals
Services específicos
RxJS
```

Somente introduza store global se a complexidade real justificar.

---

# 30. PWA

O Zelloa Family é PWA.

Entretanto:

> Não é offline-first.

Nunca considere cache local como autoridade para:

- preço;
- disponibilidade;
- pedido;
- pagamento;
- status financeiro.

Em caso de dúvida:

> consulte a API.

---

# 31. API COMO AUTORIDADE

Frontend nunca será autoridade sobre:

```text
Price
Payment
Permission
Tenant
Order state
Availability
Financial status
```

A API sempre deverá revalidar informações críticas.

---

# 32. VALORES FINANCEIROS

Nunca utilize:

```text
float
double
```

Utilize:

```text
decimal
```

ou Value Object financeiro apropriado.

Moeda inicial:

```text
BRL
```

---

# 33. PAGAMENTOS

Payments e Orders são conceitos separados.

Não misture lógica específica do PSP dentro do Aggregate Order.

---

# 34. PAYMENT GATEWAY

A Application define abstração.

Infrastructure implementa provider.

Fluxo:

```text
Application
    ↓
IPaymentGateway
    ↓
Infrastructure
    ↓
PSP
```

Orders não conhece implementação do PSP.

---

# 35. WEBHOOK

Nunca implemente:

```text
Webhook
   ↓
order.Status = Paid
```

O fluxo deverá contemplar:

```text
Receive
 ↓
Authenticate
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
Atomic persistence
```

---

# 36. IDEMPOTÊNCIA

Operações financeiras deverão ser idempotentes quando aplicável.

Eventos duplicados não poderão provocar:

- pagamento duplicado;
- transição duplicada;
- entrega duplicada;
- efeitos colaterais duplicados.

Utilize proteção no banco quando possível.

---

# 37. CONCORRÊNCIA

Considere explicitamente concorrência em:

- pagamento;
- webhook;
- reconciliação;
- transições operacionais.

Não dependa exclusivamente de:

```csharp
if (!processed)
{
    processed = true;
}
```

quando duas requisições puderem executar simultaneamente.

Utilize constraints/transações/concurrency control apropriados.

---

# 38. TRANSAÇÕES

Operações que precisem ser atomicamente consistentes deverão utilizar transação apropriada.

Exemplo:

```text
Confirm Payment
+
Mark Order Paid
+
Mark Webhook Processed
```

Não permita persistência parcial.

---

# 39. RECONCILIAÇÃO

Não trate webhook como mecanismo infalível.

Pagamentos pendentes deverão poder ser reconciliados consultando o PSP.

---

# 40. DATAS

Prefira:

```csharp
DateTimeOffset
```

para instantes.

Utilize:

```csharp
TimeProvider
```

quando regras dependerem do tempo.

Evite:

```csharp
DateTime.Now
DateTime.UtcNow
```

espalhados pelo domínio/application quando `TimeProvider` puder ser injetado.

---

# 41. TIMEZONE

Regras relacionadas ao horário da escola deverão considerar timezone institucional.

Não assumir que servidor e escola possuem mesmo timezone.

---

# 42. ERRORS

Utilize abordagem consistente.

Falhas esperadas poderão utilizar:

```text
Result<T>
```

com categorias como:

```text
Validation
NotFound
Conflict
Forbidden
BusinessRule
```

Erros HTTP deverão utilizar Problem Details.

---

# 43. EXCEPTIONS

Não utilize Exceptions para fluxo normal esperado.

Exceptions são apropriadas para:

- estado impossível;
- falha inesperada;
- infraestrutura;
- violação grave de invariantes quando apropriado.

---

# 44. LOGGING

Utilize structured logging.

Prefira:

```csharp
logger.LogInformation(
    "Order {OrderId} confirmed for tenant {TenantId}",
    orderId,
    tenantId);
```

Evite concatenação de strings.

---

# 45. DADOS SENSÍVEIS

Nunca registre:

- passwords;
- tokens completos;
- secrets;
- credentials;
- QR Code completo sem necessidade;
- payload financeiro sensível desnecessário.

---

# 46. OBSERVABILIDADE

Preserve:

```text
CorrelationId
TraceId
TenantId
```

quando apropriado.

Código deverá permanecer compatível com OpenTelemetry.

---

# 47. HTTP CLIENT

Integrações externas deverão utilizar infraestrutura apropriada de HTTP client.

Aplicar:

- timeout;
- CancellationToken;
- resiliência apropriada.

Nunca executar retry cego em operação financeira não idempotente.

---

# 48. CANCELLATION TOKEN

Propague `CancellationToken` em operações assíncronas de I/O.

Evite ignorá-lo.

---

# 49. ASYNC

Não utilize:

```csharp
.Result
.Wait()
.GetAwaiter().GetResult()
```

em fluxo normal assíncrono.

---

# 50. DEPENDENCY INJECTION

Utilize constructor injection.

Não utilize Service Locator.

Não acesse container DI diretamente dentro de regra de negócio.

---

# 51. SOLID

Utilize SOLID como orientação.

Não transforme SOLID em justificativa para dezenas de interfaces desnecessárias.

---

# 52. DRY

Não abstraia cedo demais.

Regra:

> duplicação pequena é melhor do que abstração incorreta.

---

# 53. KISS

Escolha a implementação mais simples que preserve:

- corretude;
- segurança;
- legibilidade;
- testes;
- manutenção.

---

# 54. YAGNI

Não implemente funcionalidades futuras.

Proibido antecipadamente sem requisito:

```text
Microservices
Kafka
RabbitMQ
Azure Service Bus
Kubernetes
API Gateway
Event Sourcing
Multiple databases
Multiple PSPs
Distributed Cache
Complex global state
Native mobile apps
```

---

# 55. NÃO CRIAR "BASE CLASSES" PREMATURAMENTE

Evite:

```text
BaseEntityService
BaseCrudService
BaseRepository
BaseController
BaseHandler
GenericManager
```

Abstrações deverão nascer de necessidades reais.

---

# 56. TESTES

Código de produção relevante deverá possuir testes apropriados.

Prioridades:

```text
Domain Tests
Application Tests
Integration Tests
Architecture Tests
Frontend Tests
```

---

# 57. DOMAIN TESTS

Testar principalmente:

- invariantes;
- estados;
- cálculos;
- Value Objects;
- comportamentos.

Não testar apenas getters/setters.

---

# 58. INTEGRATION TESTS

Utilize PostgreSQL realista.

Preferência:

```text
Testcontainers
```

Não confie exclusivamente em EF Core InMemory para validar comportamento de PostgreSQL.

---

# 59. ARCHITECTURE TESTS

Sempre preservar regras de dependência.

Se um Architecture Test falhar:

> corrija arquitetura.

Não remova o teste para permitir build.

---

# 60. TESTES FINANCEIROS

Obrigatórios quando módulo financeiro existir:

```text
valid payment
invalid payment
duplicate webhook
invalid webhook
wrong amount
unknown transaction
already confirmed payment
concurrent confirmation
PSP timeout
reconciliation
```

---

# 61. NÃO DESABILITAR TESTES

Não utilizar sem justificativa explícita:

```text
Skip
Ignore
Comment out
```

para esconder falhas.

---

# 62. BUILD

Uma fase não está concluída se:

```text
build fails
tests fail
migration fails
lint fails
```

Não avance.

---

# 63. WARNINGS

Backend deverá utilizar:

```xml
<TreatWarningsAsErrors>true</TreatWarningsAsErrors>
```

Não silencie warning apenas para passar build sem analisar causa.

---

# 64. DEPENDÊNCIAS EXTERNAS

Antes de adicionar pacote:

1. identifique problema;
2. verifique solução nativa;
3. justifique pacote;
4. avalie manutenção;
5. utilize versão compatível.

Não instale biblioteca para resolver problema trivial.

---

# 65. DESENVOLVIMENTO POR FASE

Siga rigorosamente:

```text
FASE 0  Foundation
FASE 1  Identity + Multi-Tenancy
FASE 2  School + Academic
FASE 3  Catalog
FASE 4  Ordering
FASE 5  Payments
FASE 6  Webhooks + Reconciliation
FASE 7  Cafeteria
FASE 8  Zelloa Family
FASE 9  Zelloa School
FASE 10 Dashboard + Audit
FASE 11 Hardening
FASE 12 Pilot
```

---

# 66. UMA FASE POR VEZ

Quando solicitado:

```text
Implemente a Fase 3
```

implemente somente a Fase 3.

Não comece a Fase 4 automaticamente.

---

# 67. ANTES DE CODIFICAR

Antes de realizar alterações significativas, apresente:

```text
FASE:
OBJETIVO:

ANÁLISE DO ESTADO ATUAL:

ARQUIVOS/ÁREAS QUE SERÃO ALTERADOS:

NOVOS ARQUIVOS PREVISTOS:

MIGRATIONS PREVISTAS:

TESTES PREVISTOS:

RISCOS/OBSERVAÇÕES:
```

Depois execute a implementação.

Se o ambiente do agente permitir execução autônoma e a instrução recebida autorizar explicitamente a fase completa, essa apresentação poderá fazer parte do relatório inicial sem exigir nova confirmação, desde que nenhuma decisão arquitetural nova seja necessária.

---

# 68. DECISÃO ARQUITETURAL NÃO PREVISTA

Caso seja necessária uma decisão que altere arquitetura:

**não decida silenciosamente.**

Apresente:

```text
PROBLEMA

OPÇÃO A
Prós:
Contras:

OPÇÃO B
Prós:
Contras:

RECOMENDAÇÃO

IMPACTO
```

Aguarde decisão quando a mudança for estrutural ou difícil de reverter.

---

# 69. NÃO REESCREVER SEM NECESSIDADE

Não substitua implementação existente apenas porque você prefere outro padrão.

Refactoring deverá possuir motivação concreta.

---

# 70. ESCOPO DE REFACTORING

Durante uma feature:

> refatore somente o necessário para implementar corretamente a feature.

Não faça limpeza geral do projeto.

---

# 71. PRESERVAR CONTRATOS

Não altere contratos públicos existentes sem analisar impacto.

Isso inclui:

- endpoints;
- DTOs;
- schemas;
- database constraints;
- eventos;
- interfaces compartilhadas.

---

# 72. ALTERAÇÃO DE CONTRATO

Caso seja necessária:

1. identifique consumidores;
2. explique impacto;
3. atualize testes;
4. atualize documentação;
5. preserve compatibilidade quando razoável.

---

# 73. NOMES

Utilize nomes claros e orientados ao domínio.

Preferir:

```text
CreateOrder
MarkOrderAsDelivered
GetCafeteriaQueue
```

Evitar:

```text
ProcessData
HandleStuff
OrderManager
GeneralService
Utils2
```

---

# 74. COMENTÁRIOS

Código deverá ser autoexplicativo sempre que possível.

Comentários deverão explicar:

> por que algo existe

e não simplesmente:

> o que a linha faz.

---

# 75. TODO

Não considere funcionalidade concluída se requisito do Gate estiver marcado apenas como TODO.

---

# 76. FRONTEND DESIGN

Interfaces deverão priorizar clareza e simplicidade.

Zelloa Family:

> poucos passos, mobile-first.

Zelloa School:

> eficiência operacional.

Não priorize efeitos visuais sobre usabilidade.

---

# 77. ACESSIBILIDADE

Utilize HTML semântico e práticas básicas de acessibilidade.

Considerar:

- labels;
- keyboard;
- contrast;
- focus;
- mensagens de erro.

---

# 78. API CONTRACT

Backend e frontend deverão possuir contrato explícito.

Evite frontend depender de estruturas internas de entidades.

Retorne DTOs apropriados.

---

# 79. NÃO EXPOR ENTIDADES DIRETAMENTE

Evite retornar Entity EF/Domain diretamente pela API.

Utilize response models.

---

# 80. SEGURANÇA DO FRONTEND

Route Guards são UX.

Não são mecanismo de segurança.

Todas as permissões críticas deverão ser revalidadas no backend.

---

# 81. IDEMPOTENCY KEY

Quando operações HTTP críticas puderem ser reenviadas pelo cliente devido a timeout/retry, avalie necessidade de idempotency key.

Especial atenção:

```text
CreateOrder
CreatePayment
```

Não introduza indiscriminadamente; documente quando necessário.

---

# 82. BANCO COMO ÚLTIMA LINHA DE DEFESA

Quando uma regra crítica puder ser protegida também pelo PostgreSQL, considere:

```text
Unique Constraint
Foreign Key
Check Constraint
Not Null
```

Não dependa apenas do código quando o banco puder preservar integridade.

---

# 83. PERFORMANCE

Não faça otimização especulativa.

Porém, evite erros evidentes:

- N+1;
- queries sem filtro;
- carregar tabelas completas;
- tracking desnecessário;
- múltiplos round trips evitáveis.

---

# 84. PAGINAÇÃO

Endpoints administrativos que possam crescer deverão possuir paginação.

Não retornar milhares de registros sem limite.

---

# 85. LGPD

Aplique minimização.

Antes de adicionar dado pessoal pergunte:

> O sistema realmente precisa desta informação para executar o caso de uso?

Se não:

> não armazene.

---

# 86. DADOS DE ALUNOS

Tenha atenção especial.

Não introduza sem requisito:

- CPF;
- endereço;
- informações médicas;
- documentos;
- dados sensíveis.

---

# 87. AUDITORIA

Operações críticas deverão permitir identificar quando apropriado:

```text
Who
What
When
Tenant
Resource
Correlation
```

---

# 88. DOCUMENTAÇÃO

Quando implementação alterar comportamento ou decisão documentada:

> atualize a documentação correspondente.

Não deixe documentação conhecida como incorreta.

---

# 89. NOVAS FEATURES

Não adicione feature ao MVP sem que ela esteja aprovada no Product Specification ou solicitada explicitamente.

---

# 90. SUGESTÕES

Você pode sugerir melhorias.

Porém:

> sugestão não é autorização para implementação.

Registre-a separadamente.

---

# 91. CHECKPOINT OBRIGATÓRIO

Ao finalizar uma fase, responda utilizando:

```text
FASE CONCLUÍDA: [número e nome]

IMPLEMENTADO
- ...

ARQUIVOS PRINCIPAIS
- ...

BANCO / MIGRATIONS
- ...

TESTES ADICIONADOS
- ...

RESULTADO DOS TESTES
- ...

RESULTADO DO BUILD
- ...

SEGURANÇA VALIDADA
- ...

PENDÊNCIAS
- ...

DECISÕES TOMADAS
- ...

ALTERAÇÕES ARQUITETURAIS
- Nenhuma
ou
- ...

SUGESTÕES PARA FUTURO
- ...

GATE DA FASE
[ATENDIDO / NÃO ATENDIDO]

PRONTO PARA PRÓXIMA FASE
[SIM / NÃO]
```

---

# 92. NÃO AVANÇAR AUTOMATICAMENTE

Mesmo com:

```text
GATE = ATENDIDO
```

não inicie a próxima fase.

Aguarde instrução.

---

# 93. QUANDO PARAR

Interrompa implementação e informe o responsável quando:

- documentação estiver conflitante;
- requisito crítico estiver ambíguo;
- decisão arquitetural nova for necessária;
- migration puder destruir dados;
- segurança puder ser comprometida;
- integração externa não estiver definida;
- teste crítico não puder ser executado;
- credencial for necessária;
- solução exigir mudança relevante de escopo.

---

# 94. QUANDO NÃO PARAR

Não interrompa desnecessariamente para decisões triviais e reversíveis.

Utilize boas práticas e documentação existente para:

- nomes locais;
- organização interna do slice;
- pequenos detalhes de implementação;
- formatação;
- testes adicionais.

---

# 95. CREDENCIAIS

Nunca invente:

```text
API keys
Passwords
Tokens
Connection strings reais
Webhook secrets
```

Utilize placeholders/configuração.

---

# 96. DADOS DE TESTE

Não utilize dados pessoais reais desnecessariamente.

Crie fixtures fictícias.

---

# 97. COMMITS

Quando solicitado a criar commits, mantenha commits pequenos e semanticamente relacionados.

Exemplo:

```text
feat(orders): implement create order slice

test(orders): add create order integration tests
```

Não misture alterações não relacionadas.

---

# 98. PRINCÍPIO DE IMPLEMENTAÇÃO

Sempre buscar:

```text
Correct
Secure
Simple
Testable
Observable
Maintainable
```

nessa ordem aproximada de importância.

---

# 99. PRINCÍPIO DO MVP

O objetivo não é criar a plataforma escolar definitiva.

O objetivo é entregar com excelência:

```text
Escolher
   ↓
Pedir
   ↓
Pagar
   ↓
Confirmar
   ↓
Preparar
   ↓
Entregar
```

---

# 100. INSTRUÇÃO DE INÍCIO DE SESSÃO

Sempre que iniciar uma nova sessão de desenvolvimento:

1. leia os documentos obrigatórios;
2. identifique a fase atual;
3. examine o estado real do repositório;
4. execute ou verifique build/testes quando apropriado;
5. compare implementação com o Gate da fase;
6. somente então proponha trabalho.

Não assuma que a última execução terminou corretamente.

---

# 101. INSTRUÇÃO PARA RETOMADA

Se receber:

```text
Continue o desenvolvimento da Zelloa.
```

não escolha automaticamente uma nova feature.

Primeiro determine:

```text
Qual foi a última fase concluída?
Qual fase está em andamento?
O Gate anterior foi atendido?
Existe código não finalizado?
Os testes estão passando?
```

Então continue exatamente daquele ponto.

---

# 102. INSTRUÇÃO PARA BUG

Quando solicitado a corrigir bug:

1. reproduza ou identifique causa;
2. determine root cause;
3. crie teste que demonstre o problema quando viável;
4. implemente correção mínima;
5. execute testes relacionados;
6. execute regressão apropriada.

Não apenas masque o sintoma.

---

# 103. INSTRUÇÃO PARA ERRO DE BUILD

Se build estiver quebrado:

> prioridade é restaurar build antes de adicionar novas funcionalidades.

Não continue acumulando código sobre uma base quebrada.

---

# 104. INSTRUÇÃO PARA TESTE FALHANDO

Primeiro determine se:

```text
código está errado
ou
teste está errado
```

Não altere expectativa automaticamente para fazer o teste passar.

---

# 105. INSTRUÇÃO PARA INTEGRAÇÕES

Antes de implementar integração externa:

1. consulte documentação oficial;
2. identifique versão da API;
3. identifique autenticação;
4. identifique idempotência;
5. identifique limites;
6. identifique webhook;
7. identifique sandbox;
8. identifique erros/retries.

Não invente contratos externos.

---

# 106. DEFINITION OF DONE DE UMA FEATURE

Uma feature somente poderá ser considerada concluída quando, conforme aplicável:

```text
✓ implementação completa
✓ validação
✓ autorização
✓ tenant isolation
✓ migration
✓ testes
✓ build
✓ lint
✓ documentação
✓ observabilidade
```

---

# 107. DEFINIÇÃO DE SUCESSO

Uma implementação bem-sucedida não é aquela que possui mais código.

É aquela que:

- resolve o caso de uso;
- respeita o domínio;
- não quebra arquitetura;
- protege dados;
- possui testes;
- é compreensível;
- pode evoluir.

---

# 108. RESTRIÇÃO FINAL

Nunca altere silenciosamente:

```text
Arquitetura
Stack
Banco
Estratégia de autenticação
Multi-tenancy
Modelo financeiro
Fluxo de pagamento
Estrutura das aplicações
Fases do projeto
```

Mudanças nesses pontos exigem decisão explícita.

---

# 109. COMANDO DE EXECUÇÃO RECOMENDADO

Ao receber uma instrução como:

```text
Implemente a Fase 0 do projeto Zelloa seguindo integralmente:

/docs/01-Zelloa-Product-Specification.md
/docs/02-Zelloa-Technical-Architecture.md
/docs/03-Zelloa-Development-Plan.md
/docs/04-Zelloa-AI-Instructions.md

Analise primeiro o estado atual do repositório.

Execute somente a Fase 0.

Não avance para a Fase 1.

Implemente, teste e valide todos os critérios do Gate da Fase 0.

Ao finalizar, apresente o checkpoint definido no documento 04.
```

o agente deverá tratar essa instrução como autorização para executar integralmente **somente a fase indicada**.

---

# 110. REGRA MÁXIMA

Quando houver dúvida entre:

> fazer mais

e

> fazer somente o necessário corretamente

escolha:

> **fazer somente o necessário corretamente.**

---

**Fim do documento — Zelloa AI Development Instructions v1.0**