# Zelloa — Payment Integration

**Documento:** 05-Zelloa-Payment-Integration  
**Versão:** 1.0  
**Status:** Especificação de pagamentos do MVP  
**Produto:** Zelloa  
**Provider candidato para o primeiro piloto:** Banco do Brasil  
**Arquitetura:** Payment Provider por Tenant

---

# 1. Objetivo

Este documento define a arquitetura financeira e de integração de pagamentos da Zelloa.

O objetivo é permitir que responsáveis realizem pagamentos por Pix e que a Zelloa confirme eletronicamente o recebimento, eliminando a necessidade de:

- envio de comprovantes;
- validação manual;
- conferência visual;
- confirmação realizada por operadores.

A Zelloa deverá atuar como plataforma de **orquestração e confirmação**, e não como custodiante dos recursos financeiros da escola.

---

# 2. Princípio Financeiro Fundamental

Sempre que tecnicamente possível:

> O dinheiro deverá ser recebido diretamente pela conta da própria instituição de ensino.

A Zelloa não deverá, como modelo padrão:

- receber dinheiro em nome da escola;
- custodiar recursos;
- acumular saldo das escolas;
- realizar repasses manuais;
- funcionar como carteira;
- funcionar como conta de pagamento.

Fluxo desejado:

```text id="d3kplw"
Responsável
     │
     │ Pix
     ▼
Instituição Financeira
     │
     ├────────────► Conta da Escola
     │
     └────────────► Zelloa
                    confirmação
```

---

# 3. Payment Provider por Tenant

Cada instituição poderá possuir sua própria configuração de recebimento.

Conceitualmente:

```text id="98kjfb"
Tenant
  │
  └── SchoolPaymentConfiguration
           │
           ├── Provider
           ├── ExternalAccountReference
           ├── Environment
           ├── Status
           └── ConfigurationReference
```

Exemplo futuro:

```text id="5xjs4w"
Escola A
→ Banco do Brasil

Escola B
→ Asaas

Escola C
→ Santander
```

Isso não significa que todos esses providers serão implementados no MVP.

---

# 4. Estratégia do MVP

Regra:

> Arquitetura preparada para múltiplos providers. Implementação inicial de apenas um provider.

O provider inicial será escolhido com base na instituição utilizada pela escola piloto.

Neste momento:

> **Banco do Brasil é o candidato principal.**

A seleção final do PSP do piloto fica pendente até a apresentação do fluxo simulado à escola e a confirmação do banco/provedor que ela utiliza e prefere. O gateway `DevelopmentSimulation` é apenas uma simulação da aplicação em ambiente de desenvolvimento; não é sandbox nem homologação de um PSP.

Antes da implementação real, essa informação deverá ser confirmada com a escola.

---

# 5. Providers Não Implementados

Não criar preventivamente:

```text id="uh9n5e"
AsaasPaymentGateway
SantanderPaymentGateway
MercadoPagoPaymentGateway
PagarMePaymentGateway
```

Adapters somente deverão ser implementados quando houver necessidade concreta.

---

# 6. Abstração

A camada Application deverá depender de uma abstração.

Exemplo conceitual:

```csharp id="8d9f0v"
public interface IPaymentGateway
{
    Task<CreatePixChargeResult> CreatePixChargeAsync(
        CreatePixChargeRequest request,
        CancellationToken cancellationToken);

    Task<PaymentStatusResult> GetPaymentStatusAsync(
        PaymentStatusRequest request,
        CancellationToken cancellationToken);

    Task<RefundPaymentResult> RefundAsync(
        RefundPaymentRequest request,
        CancellationToken cancellationToken);
}
```

A interface definitiva deverá refletir somente operações efetivamente necessárias.

Não adicionar métodos especulativos.

---

# 7. Seleção do Provider

A seleção deverá considerar o tenant atual.

Fluxo conceitual:

```text id="9b6uv7"
TenantContext
      │
      ▼
PaymentConfiguration
      │
      ▼
Provider
      │
      ▼
PaymentGateway
```

Application não deverá possuir:

```csharp id="gtmco6"
if (provider == "BancoDoBrasil")
{
   ...
}
else if (provider == "Asaas")
{
   ...
}
```

espalhado pelos casos de uso.

A resolução do adapter deverá ocorrer em infraestrutura apropriada.

---

# 8. Payment Gateway Resolver

Quando houver mais de um provider implementado, poderá existir uma abstração equivalente a:

```csharp id="7l6a50"
public interface IPaymentGatewayResolver
{
    IPaymentGateway Resolve(PaymentProvider provider);
}
```

Entretanto:

> Não criar resolver complexo enquanto existir apenas um provider implementado.

O design deverá permanecer evolutivo.

---

# 9. Payment Provider

Representar providers através de conceito controlado.

Exemplo:

```csharp id="ksht6j"
public enum PaymentProvider
{
    BancoDoBrasil = 1
}
```

Novos valores somente deverão ser adicionados quando o provider correspondente for suportado.

Não criar enum com dezenas de bancos antecipadamente.

---

# 10. School Payment Configuration

Criar configuração financeira separada das demais configurações da escola.

Modelo conceitual:

```text id="t3zylv"
SchoolPaymentConfiguration

Id
TenantId
Provider
ExternalAccountReference
Environment
Status
CreatedAt
UpdatedAt
```

Possíveis estados:

```text id="a4rhld"
PendingConfiguration
Active
Inactive
ConfigurationError
```

---

# 11. Credenciais

Credenciais não deverão ser armazenadas em texto puro na tabela de configuração.

Exemplos:

- ClientId;
- ClientSecret;
- API Key;
- certificates;
- private keys;
- webhook secrets.

Utilizar mecanismo seguro de configuração/secret storage.

A tabela poderá possuir referência ao segredo, nunca necessariamente o segredo.

Exemplo:

```text id="umjjdh"
ConfigurationReference
```

---

# 12. Ambientes

Integrações deverão diferenciar:

```text id="i7phcr"
Sandbox / Homologation
Production
```

Nunca utilizar credenciais de produção durante desenvolvimento local.

---

# 13. Banco do Brasil

Banco do Brasil é o primeiro candidato para o piloto.

A implementação deverá ocorrer somente após:

1. confirmação da instituição utilizada pela escola;
2. confirmação de que a conta/produto da escola suporta a integração necessária;
3. obtenção do acesso ao portal/API correspondente;
4. criação das credenciais;
5. configuração do ambiente de homologação;
6. entendimento definitivo do contrato atual da API.

---

# 14. Não Assumir Contrato Externo

Antes de implementar Banco do Brasil:

> Consultar documentação oficial atual.

Não codificar endpoints, schemas, autenticação ou regras baseando-se exclusivamente neste documento.

APIs externas podem mudar.

---

# 15. Capacidades Necessárias do Provider

O provider escolhido deverá oferecer, diretamente ou através de mecanismo equivalente:

- geração de cobrança Pix;
- identificador único da cobrança;
- QR Code ou dados para sua geração;
- Pix Copia e Cola;
- consulta da cobrança;
- confirmação eletrônica;
- mecanismo de webhook ou equivalente;
- ambiente de homologação quando disponível.

Desejável:

- expiração;
- devolução;
- consulta de recebimentos;
- webhook configurável;
- idempotência.

---

# 16. Payment

Payment deverá possuir ciclo de vida separado de Order.

Modelo conceitual:

```text id="k0oy71"
Payment

Id
TenantId
OrderId

Provider
ExternalTransactionId

Amount
Currency

Status

CreatedAt
ExpiresAt
ConfirmedAt

PixCopyPaste
QrCodeReference
```

Campos definitivos deverão refletir necessidade real e segurança.

---

# 17. Payment Status

Estados conceituais:

```text id="svmh1r"
Pending
Confirmed
Expired
Cancelled
Refunded
Failed
```

Não confundir `Payment.Status` com `Order.Status`.

---

# 18. Order x Payment

Exemplo:

```text id="naw0ms"
ORDER                     PAYMENT

AwaitingPayment           Pending
       │                     │
       │                     ▼
       │                 Confirmed
       │                     │
       ▼                     │
Paid ◄───────────────────────┘
```

Pagamento confirmado provoca transição válida do pedido.

---

# 19. Regra Fundamental

Apenas mecanismo confiável poderá executar:

```text id="5cxkkf"
Payment → Confirmed
```

Exemplos:

- webhook autenticado;
- reconciliação com consulta ao provider.

Nunca:

- frontend;
- comprovante;
- operador;
- QR Code;
- botão "já paguei".

---

# 20. Criação do Pedido

Fluxo:

```text id="zd2hd7"
Guardian
   │
   ▼
CreateOrder
   │
   ├── validar aluno
   ├── validar vínculo
   ├── validar produtos
   ├── calcular valores
   └── persistir Order
             │
             ▼
      AwaitingPayment
```

---

# 21. Criação da Cobrança

Depois do Order:

```text id="q9lr85"
Order
   │
   ▼
CreatePixCharge
   │
   ▼
Payment
   │
   ▼
PaymentGateway
   │
   ▼
Provider
   │
   ▼
Pix Charge
```

---

# 22. Valor da Cobrança

O valor enviado ao provider deverá vir do valor calculado e persistido pelo backend.

Nunca utilizar:

```text id="7ib1h7"
request.Amount
```

proveniente diretamente do frontend como autoridade.

Fluxo:

```text id="6eyicn"
Order.Total
    ↓
Payment.Amount
    ↓
Provider
```

---

# 23. Identificador Externo

Toda cobrança deverá possuir identificador que permita correlacionar:

```text id="06sp9u"
Zelloa Payment
        ↕
Provider Transaction
```

Esse identificador deverá ser persistido.

---

# 24. Pix Dinâmico

Preferir Pix dinâmico individual por pedido.

Evitar QR Code estático.

Cada pedido deverá possuir cobrança identificável.

Isso permite correlacionar automaticamente:

```text id="ghwxgk"
Pagamento X
     ↓
Pedido Y
     ↓
Aluno Z
```

---

# 25. Dados Retornados ao Family

A API poderá retornar ao frontend:

```text id="gtd6yx"
PaymentId
Amount
Status
PixCopyPaste
QrCode
ExpiresAt
```

Nunca retornar:

- ClientSecret;
- API Key;
- certificado;
- credencial bancária.

---

# 26. QR Code

O QR Code poderá ser:

- retornado pelo provider;
- gerado a partir de payload oficial quando apropriado.

A estratégia dependerá do provider.

O frontend deverá apenas exibi-lo.

---

# 27. Pix Copia e Cola

Deverá existir opção:

```text id="8nqigz"
Copiar código Pix
```

especialmente importante para experiência mobile.

---

# 28. Expiração

Cobranças deverão possuir prazo quando suportado.

Ao expirar:

```text id="yjz1ut"
Payment
Pending
   ↓
Expired
```

e, conforme regra de negócio:

```text id="b7aw4c"
Order
AwaitingPayment
   ↓
PaymentExpired
```

---

# 29. Webhook

O provider deverá notificar a Zelloa quando possível.

Endpoint conceitual:

```text id="2gdn65"
POST /api/webhooks/payments/{provider}
```

A URL definitiva poderá variar conforme necessidade técnica.

---

# 30. Segurança do Webhook

Antes de processar:

1. validar origem/autenticidade conforme mecanismo oficial;
2. validar formato;
3. identificar provider;
4. identificar evento;
5. verificar idempotência;
6. localizar Payment;
7. validar valores;
8. validar tenant quando aplicável;
9. executar transição.

---

# 31. Não Confiar Cegamente no Payload

Mesmo webhook autenticado deverá ser tratado como entrada externa.

Validar:

```text id="xtdpr3"
ExternalTransactionId
Amount
Status
Payment reference
```

conforme capacidade do provider.

---

# 32. PaymentWebhookEvent

Persistir controle de processamento.

Modelo conceitual:

```text id="z4k8fb"
PaymentWebhookEvent

Id
Provider
ExternalEventId
ExternalTransactionId
ReceivedAt
ProcessedAt
Status
PayloadHash
FailureReason
```

Não persistir payload completo indiscriminadamente.

---

# 33. Payload

Antes de armazenar payload bruto de instituição financeira:

> verificar necessidade e dados contidos.

Preferir persistir somente informações necessárias.

LGPD e segurança deverão ser consideradas.

---

# 34. Idempotência

Evento duplicado deverá produzir:

```text id="gslt8g"
Primeiro evento
→ processado

Segundo evento igual
→ reconhecido como duplicado
→ nenhum efeito financeiro adicional
```

---

# 35. Unique Constraint

Sempre que o provider oferecer identificador único confiável:

```text id="dx8v7q"
Provider + ExternalEventId
```

deverá possuir proteção de unicidade apropriada.

---

# 36. Confirmação do Pagamento

Após validação:

```text id="0jvjuw"
Payment.MarkAsConfirmed(...)
```

Depois:

```text id="i77hwq"
Order.MarkAsPaid(...)
```

A transição deverá respeitar invariantes.

---

# 37. Atomicidade

Quando tecnicamente apropriado:

```text id="t8vwdp"
Payment Confirmed
+
Order Paid
+
Webhook Processed
```

deverão ser persistidos na mesma unidade transacional.

---

# 38. Concorrência

Considere:

```text id="jyratc"
Webhook
      ↘
        Payment
      ↗
Reconciliation
```

executando simultaneamente.

A operação deverá continuar idempotente.

---

# 39. Reconciliação

Webhook não é considerado mecanismo infalível.

Deverá existir mecanismo:

```text id="gmdknf"
Pending Payments
      │
      ▼
Provider API
      │
      ▼
Current Status
      │
      ▼
Reconciliation
```

---

# 40. Casos de Reconciliação

Exemplo:

```text id="p2swa5"
Responsável pagou
       ↓
Banco confirmou
       ↓
Webhook falhou
       ↓
Zelloa continua Pending
       ↓
Reconciliation Job
       ↓
Consulta Banco
       ↓
Confirmed
       ↓
Order Paid
```

---

# 41. Polling do Frontend

Zelloa Family poderá consultar periodicamente:

```text id="qtx1fx"
GET payment status
```

Porém:

> polling do frontend não confirma pagamento.

Ele apenas consulta o estado que o backend considera confiável.

---

# 42. WebSocket

Não introduzir WebSocket inicialmente.

Polling controlado é suficiente para o MVP.

Reavaliar somente se experiência justificar.

---

# 43. Cancelamento Antes do Pagamento

Pedido ainda não pago poderá ser cancelado conforme regras do produto.

Quando suportado, cobrança correspondente poderá ser cancelada.

---

# 44. Cancelamento Após Pagamento

Nunca assumir:

```text id="ohsqas"
Cancel Order
=
Refund Payment
```

São operações distintas.

Pedido pago exige fluxo específico.

---

# 45. Refund

Quando suportado pelo provider, poderá existir:

```text id="kagcx3"
RefundPayment
```

Fluxo:

```text id="at8e11"
Solicitação
   ↓
Autorização
   ↓
Provider
   ↓
Refund
   ↓
Confirmação
   ↓
Payment = Refunded
```

---

# 46. Refund no MVP

Não criar fluxo administrativo complexo de refund sem necessidade do piloto.

Entretanto, arquitetura de Payment não deverá assumir que pagamento confirmado é irreversível.

---

# 47. Falha na Criação da Cobrança

Exemplo:

```text id="x27gtz"
Order criado
     ↓
PSP indisponível
     ↓
Charge falha
```

Não marcar pedido como pago.

Registrar falha adequadamente.

Permitir estratégia segura de nova tentativa.

---

# 48. Retry

Retries somente quando seguros.

Não executar retry automático indiscriminado na criação de cobranças.

Quando provider oferecer idempotency key, utilizar conforme documentação oficial.

---

# 49. Idempotência da Criação da Cobrança

Deverá ser avaliada para evitar:

```text id="klqkwz"
1 Order
   ↓
3 requisições
   ↓
3 cobranças diferentes
```

Ideal:

```text id="x0rrgj"
1 Order
   ↓
1 cobrança ativa
```

salvo regra explícita de regeneração.

---

# 50. Timeout

Timeout não significa necessariamente falha da operação externa.

Exemplo:

```text id="n6s6da"
Zelloa
  │
  ├── create charge
  │
  ▼
Banco cria cobrança
  │
  X conexão cai
```

A Zelloa não pode assumir automaticamente que a cobrança não foi criada.

A estratégia deverá utilizar mecanismos de idempotência/consulta oferecidos pelo provider.

---

# 51. Estado Incerto

Quando não for possível determinar resultado:

```text id="iybd1j"
Payment
→ estado de processamento apropriado
```

ou estratégia equivalente.

Não gerar nova cobrança cegamente.

---

# 52. Provider Indisponível

O sistema deverá apresentar mensagem amigável:

```text id="yyj8v8"
Não foi possível gerar o pagamento neste momento.
Tente novamente em alguns instantes.
```

Detalhes técnicos ficam nos logs.

---

# 53. Auditoria Financeira

Registrar quando apropriado:

```text id="p6sf8p"
PaymentId
OrderId
TenantId
Provider
ExternalTransactionId
Amount
OldStatus
NewStatus
Timestamp
CorrelationId
```

---

# 54. Logging

Exemplo:

```text id="zy1p9r"
Payment {PaymentId} confirmed for Order {OrderId}
```

Não registrar credenciais.

---

# 55. Observabilidade

Monitorar:

- criação de cobranças;
- falhas;
- latência do provider;
- webhooks;
- duplicidades;
- reconciliações;
- divergência de valores.

---

# 56. Divergência de Valor

Se:

```text id="d11ap7"
Expected = R$ 15,00
Received = R$ 10,00
```

não marcar pedido automaticamente como pago.

Registrar inconsistência para investigação.

---

# 57. Pagamento Desconhecido

Webhook para transação desconhecida:

- não criar pedido;
- não associar automaticamente;
- registrar evento;
- responder adequadamente ao provider;
- permitir investigação.

---

# 58. Testes Obrigatórios

Antes do piloto:

```text id="k9k17f"
✓ criação de cobrança válida
✓ cobrança inválida
✓ provider indisponível
✓ timeout
✓ webhook válido
✓ webhook inválido
✓ webhook duplicado
✓ pagamento inexistente
✓ valor divergente
✓ pagamento já confirmado
✓ pagamento expirado
✓ concorrência webhook/reconciliation
✓ reconciliação
✓ isolamento entre tenants
```

---

# 59. Fake Provider para Testes

Poderá existir implementação controlada exclusivamente para testes.

Exemplo:

```text id="xkmtsm"
FakePaymentGateway
```

Seu objetivo é facilitar testes automatizados.

Nunca deverá estar habilitado em produção.

## Simulação do fluxo em desenvolvimento

Além do fake provider restrito aos testes automatizados, a aplicação poderá oferecer um gateway simulado em ambiente de desenvolvimento para demonstrar o fluxo de pagamento antes da escolha e integração do PSP do piloto.

Regras obrigatórias:

- usar a abstração `IPaymentGateway`, sem chamadas a instituições financeiras;
- não movimentar dinheiro nem afirmar que houve liquidação real;
- usar o estado distinto `SimulatedConfirmed` para a confirmação simulada; não preencher `ConfirmedAt` nem mudar o pedido para `Paid`;
- identificar cobranças e confirmações simuladas na API e na interface que as apresentar;
- habilitar somente em ambiente de desenvolvimento; falhar de forma segura se configurado em produção;
- manter credenciais e operações reais do PSP completamente separadas;
- não considerar a simulação suficiente para o Gate da Fase 5.

Manter o gateway para a apresentação da funcionalidade à escola e para validação do fluxo de demonstração. Após a escola informar o provedor preferido e a integração real ser validada em homologação, remover o gateway de simulação da aplicação como débito técnico. O fake provider usado somente por testes automatizados poderá continuar existindo.

---

# 60. Banco do Brasil — Estrutura Futura

Caso confirmado para o piloto:

```text id="xyjxtb"
Infrastructure/
└── Payments/
    └── BancoDoBrasil/
        ├── BancoDoBrasilPaymentGateway.cs
        ├── BancoDoBrasilClient.cs
        ├── BancoDoBrasilOptions.cs
        ├── Authentication/
        └── Models/
```

A estrutura definitiva dependerá da API oficial vigente.

---

# 61. Banco do Brasil — Onboarding

Antes de ativar uma escola no provider Banco do Brasil, levantar:

```text id="ykmzw9"
1. A escola possui conta no Banco do Brasil?

2. A conta é PJ?

3. Qual solução Pix/cobrança é utilizada?

4. Existe acesso administrativo necessário?

5. A escola pode habilitar API Pix?

6. Quem é o responsável financeiro?

7. Quem poderá autorizar integração?

8. Quais credenciais/certificados serão necessários?

9. Existe homologação disponível para aquele contrato?

10. O recebimento será creditado diretamente na conta da escola?
```

Não solicitar senhas bancárias pessoais.

---

# 62. Onboarding Técnico

Fluxo conceitual:

```text id="oyynmt"
SchoolAdmin
      │
      ▼
Configurar Pagamentos
      │
      ▼
Selecionar Provider
      │
      ▼
Processo de conexão/autorização
      │
      ▼
Validação
      │
      ▼
Teste
      │
      ▼
Active
```

No primeiro piloto, parte desse processo poderá ser assistida manualmente pela equipe Zelloa.

Não é necessário automatizar todo onboarding no MVP.

---

# 63. Status da Configuração

Exemplo:

```text id="w37h2k"
PendingConfiguration
       ↓
Testing
       ↓
Active
```

ou:

```text id="9wsgqm"
ConfigurationError
```

Estados definitivos deverão refletir necessidade real.

---

# 64. Troca de Provider

Futuramente uma escola poderá mudar:

```text id="lsvg5l"
Banco A
   ↓
Banco B
```

Pagamentos históricos deverão preservar o provider utilizado originalmente.

Portanto:

```text id="0ej87f"
Payment.Provider
```

é snapshot da integração utilizada naquela transação.

---

# 65. Não Reinterpretar Histórico

Se escola mudar de provider:

```text id="5qldzb"
Payment antigo
Provider = BancoDoBrasil
```

continua BancoDoBrasil.

Nunca atualizar pagamentos históricos para novo provider.

---

# 66. Múltiplos Providers Simultâneos

Fora do MVP.

Uma escola terá:

> uma configuração ativa de recebimento para o fluxo de cantina.

Não implementar roteamento financeiro complexo.

---

# 67. Split

Split de pagamento está fora do MVP.

Não implementar:

```text id="ekbh6h"
Escola → percentual
Zelloa → percentual
```

no primeiro piloto.

---

# 68. Monetização da Zelloa

Inicialmente, a monetização deverá ser independente do pagamento do lanche.

Exemplo:

```text id="n70cp7"
Escola paga mensalidade da Zelloa
```

e:

```text id="wbdug6"
Responsável paga lanche
        ↓
Conta da escola
```

Isso simplifica o modelo financeiro.

---

# 69. Evolução Comercial

Futuramente poderão ser avaliados:

- mensalidade SaaS;
- cobrança por aluno;
- cobrança por transação;
- split;
- planos;
- módulos adicionais.

Nenhum desses modelos deverá alterar prematuramente o MVP.

---

# 70. Segurança

Obrigatório:

```text id="pbwtxx"
HTTPS
Secret Management
Authentication
Authorization
Tenant Isolation
Webhook Authentication
Input Validation
Audit
Structured Logging
```

---

# 71. LGPD

Persistir apenas dados financeiros necessários.

Evitar armazenar dados bancários que não sejam essenciais.

Não armazenar:

- senha bancária;
- credencial de internet banking;
- informações desnecessárias da conta.

---

# 72. PCI

O MVP não deverá manipular dados de cartão.

Forma inicial:

```text id="l3dixk"
Pix
```

Isso reduz significativamente o escopo financeiro e de compliance.

---

# 73. Endpoint Conceitual — Criar Pagamento

Possível contrato:

```text id="vsgp3l"
POST /api/orders/{orderId}/payments/pix
```

Resposta conceitual:

```json id="kn2e2n"
{
  "paymentId": "...",
  "orderId": "...",
  "amount": 15.00,
  "currency": "BRL",
  "status": "Pending",
  "pixCopyPaste": "...",
  "qrCode": "...",
  "expiresAt": "..."
}
```

Contrato definitivo será definido no documento 06.

---

# 74. Endpoint Conceitual — Status

```text id="u4k8ve"
GET /api/payments/{paymentId}
```

Resposta:

```json id="7a4xby"
{
  "paymentId": "...",
  "status": "Confirmed",
  "confirmedAt": "..."
}
```

---

# 75. Endpoint Conceitual — Webhook

```text id="wrllc5"
POST /api/webhooks/payments/{provider}
```

Esse endpoint não utiliza autenticação normal de usuário.

Utiliza mecanismo de autenticação/validação específico do provider.

---

# 76. Relação com o Development Plan

Fase 5:

```text id="hw89cb"
Payment
CreatePixCharge
PaymentGateway
Provider integration
```

Fase 6:

```text id="cvs4f5"
Webhook
Idempotency
Confirmation
Reconciliation
Concurrency
```

Não misturar as duas fases sem necessidade.

---

# 77. Decisão Registrada

Decisão arquitetural:

> Payment Provider será configurável por Tenant.

Decisão de implementação:

> Apenas um provider será implementado inicialmente.

Candidato atual:

> Banco do Brasil.

Decisão financeira:

> A Zelloa deverá priorizar recebimento direto pela escola.

---

# 78. Critério para Escolha Definitiva do Primeiro Provider

Antes da Fase 5:

```text id="quc8ca"
[ ] confirmar banco da escola piloto
[ ] confirmar conta PJ
[ ] confirmar capacidade Pix/API
[ ] verificar documentação oficial
[ ] verificar homologação
[ ] verificar webhook
[ ] verificar autenticação
[ ] verificar custos
[ ] verificar requisitos contratuais
[ ] validar recebimento direto
```

Somente depois:

```text id="0hqnbw"
Provider inicial = CONFIRMADO
```

---

# 79. Definition of Done — Payment Integration

A integração financeira do MVP será considerada pronta quando:

```text id="c0apfc"
✓ escola possui provider configurado

✓ Zelloa cria cobrança Pix

✓ cobrança está associada ao Order

✓ valor é definido pelo backend

✓ Pix pode ser pago

✓ dinheiro é destinado à escola

✓ confirmação eletrônica chega à Zelloa

✓ webhook é autenticado

✓ processamento é idempotente

✓ Payment é confirmado

✓ Order é marcado como Paid

✓ pedido aparece na cantina

✓ webhook perdido pode ser reconciliado

✓ eventos duplicados não provocam efeitos duplicados

✓ divergências financeiras são detectadas

✓ isolamento entre tenants é validado

✓ credenciais estão protegidas

✓ testes financeiros estão passando
```

---

# 80. Princípio Final

A responsabilidade da Zelloa é:

> saber **quem deve pagar**, **quanto deve pagar**, **por qual pedido**, solicitar a cobrança à instituição financeira e confirmar eletronicamente o resultado.

A responsabilidade da instituição financeira é:

> movimentar e custodiar o dinheiro.

A responsabilidade da escola é:

> ser a beneficiária do pagamento referente ao serviço vendido.

Essas responsabilidades não deverão ser misturadas sem decisão arquitetural, jurídica e comercial explícita.

---

**Fim do documento — Zelloa Payment Integration v1.0**
