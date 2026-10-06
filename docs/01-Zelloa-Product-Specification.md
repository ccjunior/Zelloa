# Zelloa — Product Specification

**Documento:** 01-Zelloa-Product-Specification  
**Versão:** 1.0  
**Status:** Draft para desenvolvimento do MVP  
**Produto:** Zelloa  
**Módulo inicial:** Alimentação Escolar  
**Objetivo:** Especificação funcional do MVP

---

# 1. Visão do Produto

A **Zelloa** é uma plataforma digital criada para simplificar a relação entre famílias e serviços oferecidos pelas instituições de ensino.

O nome é inspirado no conceito de **zelo**: cuidado com as pessoas, com o tempo e com cada detalhe da experiência escolar.

A plataforma nasce a partir de um problema real identificado na operação de cantinas escolares: pedidos realizados por WhatsApp, pagamentos via Pix e validação manual de comprovantes.

Esse processo pode gerar:

- comprovantes falsificados;
- dificuldade de conciliação dos pagamentos;
- trabalho manual para funcionários;
- pedidos liberados sem confirmação financeira;
- dificuldade para organizar pedidos por aluno e turma;
- ausência de histórico estruturado;
- dificuldade de acompanhamento pelos responsáveis.

O primeiro módulo da Zelloa será voltado à **alimentação escolar**, permitindo que responsáveis façam pedidos antecipados, realizem o pagamento digitalmente e que a escola receba para preparação somente pedidos cujo pagamento tenha sido efetivamente confirmado.

A arquitetura do produto deverá permitir que, futuramente, outros serviços escolares possam ser incorporados sem alterar o propósito da marca.

---

# 2. Proposta de Valor

## Para a escola

Centralizar pedidos, pagamentos e operação da cantina em uma única plataforma.

Principais benefícios:

- redução de fraudes relacionadas a comprovantes;
- confirmação automática de pagamentos;
- redução da conferência manual;
- organização dos pedidos;
- rastreabilidade;
- acompanhamento financeiro;
- melhoria da operação da cantina.

## Para os responsáveis

Permitir a compra do lanche do aluno de maneira rápida e segura pelo celular.

Principais benefícios:

- não depender de atendimento pelo WhatsApp;
- visualizar o cardápio;
- saber antecipadamente o valor;
- pagamento integrado;
- confirmação do pedido;
- histórico de compras;
- facilidade para repetir pedidos.

## Para a cantina

Transformar pedidos dispersos em uma fila operacional estruturada.

A cantina deverá saber:

- quem é o aluno;
- qual sua turma;
- quais produtos foram comprados;
- se o pagamento foi confirmado;
- quando o pedido foi realizado;
- qual o status da preparação;
- se o pedido já foi entregue.

---

# 3. Princípio Fundamental

A Zelloa deverá obedecer à seguinte regra:

> **Nenhum pedido poderá ser disponibilizado para preparação como pedido confirmado sem que o pagamento tenha sido validado por uma fonte financeira confiável.**

Comprovantes enviados pelo responsável não serão considerados confirmação de pagamento.

O frontend também não poderá determinar que um pedido foi pago.

A confirmação deverá ocorrer por meio da integração financeira definida para a plataforma.

---

# 4. Escopo do MVP

O MVP deverá suportar o fluxo completo:

**Responsável → Aluno → Cardápio → Pedido → Pagamento → Confirmação → Cantina → Preparação → Entrega**

O sistema deverá possuir quatro áreas funcionais principais:

1. Portal do Responsável;
2. Operação da Cantina;
3. Administração Escolar;
4. Pagamentos.

---

# 5. Atores

## 5.1 Responsável

Pessoa autorizada a realizar pedidos para um ou mais alunos.

Poderá:

- acessar sua conta;
- visualizar alunos vinculados;
- consultar o cardápio;
- criar pedidos;
- realizar pagamentos;
- acompanhar pedidos;
- consultar histórico;
- repetir pedidos anteriores.

---

## 5.2 Funcionário da Cantina

Responsável pela operação dos pedidos.

Poderá:

- visualizar pedidos confirmados;
- visualizar aluno e turma;
- visualizar itens;
- iniciar preparação;
- marcar pedido como pronto;
- marcar pedido como entregue;
- consultar pedidos do dia.

O funcionário da cantina não deverá precisar validar comprovantes manualmente.

---

## 5.3 Administrador Escolar

Responsável pelas configurações da escola.

Poderá:

- administrar alunos;
- administrar responsáveis;
- convidar responsáveis para ativar a própria conta;
- administrar turmas;
- administrar produtos;
- administrar categorias;
- configurar preços;
- controlar disponibilidade;
- consultar pedidos;
- consultar pagamentos;
- visualizar indicadores operacionais.

---

## 5.4 Administrador Zelloa

Perfil interno da plataforma.

Será responsável por:

- cadastrar instituições;
- configurar tenants;
- administrar acessos administrativos;
- convidar o administrador inicial de cada escola para ativar a conta;
- acompanhar integrações;
- consultar problemas técnicos;
- realizar operações de suporte autorizadas.

Este perfil deverá possuir acesso altamente restrito.

---

# 6. Multi-Instituição

Embora o piloto inicial possa utilizar somente uma escola, o produto deverá nascer preparado para múltiplas instituições.

Cada instituição será tratada como um **tenant**.

Dados pertencentes a uma instituição não poderão ser acessados por outra.

Exemplos:

**Escola A**

- alunos da Escola A;
- responsáveis da Escola A;
- produtos da Escola A;
- pedidos da Escola A;
- pagamentos da Escola A.

**Escola B**

Deverá possuir ambiente logicamente isolado.

A implementação técnica será definida no documento de arquitetura.

---

# 7. Módulo — Instituição

O sistema deverá permitir representar uma instituição de ensino.

Informações mínimas:

- nome;
- nome fantasia;
- identificador;
- status;
- dados de contato;
- configurações operacionais;
- timezone.

No MVP, cada tenant poderá possuir inicialmente uma unidade.

A estrutura deverá evitar impedir suporte futuro a múltiplas unidades.

---

# 8. Módulo — Turmas

A instituição poderá cadastrar suas turmas.

Informações mínimas:

- nome;
- série/nível;
- turno;
- ano letivo;
- status.

Exemplo:

**Infantil 5 — Turma A — Matutino**

A turma será utilizada principalmente para identificação do aluno durante a entrega.

---

# 9. Módulo — Alunos

Cada aluno deverá pertencer a uma instituição e estar associado a uma turma ativa.

Informações mínimas:

- nome;
- identificador interno;
- turma;
- status.

O MVP deverá aplicar o princípio de minimização de dados.

Não deverão ser solicitadas informações pessoais do aluno que não sejam necessárias para a operação.

---

# 10. Módulo — Responsáveis

Um responsável poderá possuir vínculo com um ou mais alunos.

Um aluno poderá possuir mais de um responsável autorizado.

O vínculo deverá ser explicitamente registrado.

O responsável somente poderá realizar operações relacionadas aos alunos aos quais possui acesso.

O administrador escolar cadastra o responsável e inicia o convite de ativação da conta. O responsável define a própria senha ao ativar o convite. A associação ao aluno é criada explicitamente pela escola; possuir uma conta, por si só, não concede acesso a nenhum aluno.

No MVP, os convites serão gerados como links individuais de uso único e entregues manualmente pelo administrador que os criou. A ativação deverá ocorrer em até 24 horas. Não haverá cadastro público nem envio automatizado por provedor externo nesta fase.

---

# 11. Módulo — Catálogo

A escola poderá manter um catálogo de produtos.

Exemplos:

- Misto quente;
- Pão de queijo;
- Suco;
- Bolo;
- Salada de frutas.

Cada produto deverá possuir, no mínimo:

- nome;
- descrição opcional;
- categoria;
- preço;
- imagem opcional;
- status;
- disponibilidade.

Produtos inativos não poderão ser adicionados a novos pedidos.

---

# 12. Categorias

Produtos poderão ser organizados por categorias.

Exemplos:

- Lanches;
- Bebidas;
- Frutas;
- Combos.

A categorização deverá facilitar a experiência de compra.

---

# 13. Disponibilidade

Um produto poderá estar:

**Disponível**

Pode ser comprado.

**Indisponível**

Continua cadastrado, mas não pode ser comprado.

A alteração da disponibilidade não deverá alterar pedidos já criados.

---

# 14. Pedido

Um pedido deverá pertencer a:

- uma instituição;
- um responsável;
- um aluno;
- uma turma de referência;
- uma data operacional.

O pedido possuirá um ou mais itens.

Cada item deverá registrar um snapshot das informações comerciais necessárias no momento da compra.

Alterações futuras no produto ou preço não deverão modificar pedidos anteriores.

---

# 15. Valores do Pedido

O backend será a autoridade para cálculo financeiro.

O frontend poderá exibir valores para experiência do usuário, mas o valor definitivo deverá ser calculado pelo servidor.

O cliente não poderá enviar um valor financeiro arbitrário e determinar o total do pedido.

O servidor deverá recuperar os produtos válidos e calcular novamente:

**Preço × Quantidade = Subtotal**

E posteriormente:

**Soma dos subtotais = Total do pedido**

---

# 16. Estados do Pedido

O pedido deverá possuir estados explícitos.

Fluxo inicial:

**Created**

Pedido criado.

↓

**AwaitingPayment**

Aguardando pagamento.

↓

**Paid**

Pagamento confirmado.

↓

**Preparing**

Em preparação.

↓

**Ready**

Pronto para entrega.

↓

**Delivered**

Entregue.

Estados alternativos:

**Cancelled**

Pedido cancelado.

**PaymentExpired**

Prazo para pagamento expirado.

O fluxo de transições deverá ser controlado pelo backend.

Não será permitido alterar livremente os estados.

---

# 17. Pagamento

Cada pedido que exigir pagamento deverá possuir uma cobrança correspondente.

Para o MVP, o meio inicial será:

**Pix**

A experiência deverá disponibilizar:

- QR Code;
- Pix Copia e Cola;
- valor;
- prazo para pagamento;
- status.

O pagamento possuirá ciclo de vida próprio, independente do pedido.

---

# 18. Confirmação de Pagamento

A confirmação deverá ocorrer por meio da integração com o provedor financeiro.

Fluxo esperado:

1. Zelloa cria o pedido.
2. Backend calcula o valor.
3. Zelloa solicita a cobrança ao provedor.
4. Provedor retorna dados da cobrança Pix.
5. Responsável realiza o pagamento.
6. Provedor identifica o recebimento.
7. Zelloa recebe a confirmação.
8. A confirmação é validada.
9. Pagamento passa para confirmado.
10. Pedido passa para `Paid`.
11. Pedido fica disponível para operação da cantina.

O sistema deverá ser resiliente a notificações duplicadas.

Receber duas notificações referentes à mesma transação não poderá:

- duplicar pagamento;
- duplicar pedido;
- alterar valor;
- provocar duas entregas;
- executar efeitos colaterais duplicados.

---

# 19. Expiração

Cobranças deverão possuir prazo de validade.

Quando uma cobrança expirar sem pagamento:

**Payment → Expired**

e

**Order → PaymentExpired**

Um pagamento confirmado não poderá posteriormente ser tratado como expirado apenas por uma rotina automática.

---

# 20. Fila da Cantina

A tela operacional deverá priorizar pedidos que precisam de ação.

Pedidos aguardando pagamento não deverão aparecer como pedidos para preparação.

Após confirmação:

**Novo pedido pago**

↓

**Preparando**

↓

**Pronto**

↓

**Entregue**

A interface deverá apresentar claramente:

- aluno;
- turma;
- itens;
- quantidades;
- horário;
- observação, quando suportada;
- status.

---

# 21. Entrega

No MVP, a confirmação de entrega será realizada manualmente pelo funcionário autorizado.

Ao selecionar **Entregue**, deverão ser registrados:

- data/hora;
- usuário responsável pela operação.

O registro deverá permanecer disponível para auditoria.

---

# 22. Portal do Responsável

A experiência deverá ser mobile-first.

Após autenticação, o responsável deverá conseguir chegar ao cardápio com o menor número razoável de interações.

Fluxo:

**Login**

↓

**Selecionar aluno**

↓

**Cardápio**

↓

**Carrinho**

↓

**Confirmar pedido**

↓

**Pix**

↓

**Pagamento confirmado**

↓

**Acompanhamento**

---

# 23. Repetir Pedido

O responsável poderá selecionar um pedido anterior e solicitar sua repetição.

Essa operação não deverá simplesmente duplicar os valores antigos.

O sistema deverá verificar novamente:

- existência dos produtos;
- disponibilidade;
- preço atual;
- regras atuais;
- horário permitido.

Um novo pedido deverá ser criado utilizando as condições comerciais atuais.

---

# 24. Horário Limite

A escola deverá poder definir um horário limite para pedidos destinados à entrega naquele período.

Exemplo:

> Pedidos para o turno da manhã podem ser realizados até 08:30.

A definição detalhada de regras por turno poderá evoluir posteriormente.

O MVP deverá, no mínimo, impedir novos pedidos quando o período configurado estiver encerrado.

---

# 25. Cancelamento

O MVP deverá diferenciar:

### Pedido ainda não pago

Poderá ser cancelado conforme as regras da escola.

### Pedido pago

Não deverá gerar estorno automaticamente sem uma política explícita.

Cancelamento e estorno são conceitos distintos.

Regras de reembolso deverão ser definidas antes da implementação de estorno financeiro automático.

---

# 26. Dashboard Administrativo

A escola deverá possuir uma visão básica da operação.

Indicadores iniciais:

- pedidos realizados hoje;
- pedidos pagos;
- pedidos aguardando pagamento;
- pedidos em preparação;
- pedidos entregues;
- pedidos cancelados;
- valor confirmado no período.

O dashboard não substitui relatórios financeiros do provedor de pagamento.

---

# 27. Histórico

A plataforma deverá manter histórico suficiente para rastrear:

- criação do pedido;
- alterações relevantes de status;
- confirmação de pagamento;
- preparação;
- entrega;
- cancelamento;
- usuário responsável por operações administrativas relevantes.

---

# 28. Segurança

O MVP deverá possuir:

- autenticação;
- autorização baseada em perfil;
- isolamento por tenant;
- validação server-side;
- proteção de endpoints administrativos;
- proteção da integração financeira;
- logs;
- tratamento centralizado de erros;
- auditoria de operações críticas.

Nenhuma autorização poderá depender exclusivamente da interface.

---

# 29. Privacidade e LGPD

A plataforma manipulará dados relacionados a responsáveis e alunos.

Deverão ser aplicados os princípios de:

- finalidade;
- necessidade;
- minimização;
- segurança;
- controle de acesso;
- rastreabilidade.

Dados de alunos deverão ser limitados ao necessário para prestação do serviço.

Aspectos jurídicos detalhados, bases legais, políticas de privacidade, retenção e contratos serão tratados separadamente antes da entrada em produção.

---

# 30. Fora do Escopo do MVP

Não deverão ser desenvolvidos nesta primeira versão, salvo revisão formal do escopo:

- aplicativo Android/iOS nativo;
- carteira pré-paga;
- mensalidade escolar;
- gestão pedagógica;
- frequência escolar;
- notas;
- controle avançado de estoque;
- marketplace;
- programa de fidelidade;
- cupons;
- assinatura recorrente de lanches;
- reconhecimento facial;
- IA para verificar comprovantes;
- integração com múltiplos PSPs simultaneamente;
- ERP escolar completo;
- módulo contábil;
- emissão fiscal avançada;
- WhatsApp como requisito para funcionamento do pedido.

A arquitetura poderá considerar evolução futura, mas **não deverão ser criadas funcionalidades especulativas**.

---

# 31. Critérios de Sucesso do MVP

O MVP será considerado funcional quando for possível executar o seguinte cenário ponta a ponta:

1. Escola está cadastrada.
2. Turma está cadastrada.
3. Aluno está cadastrado.
4. Responsável está vinculado ao aluno.
5. Produtos estão disponíveis.
6. Responsável acessa a plataforma.
7. Seleciona o aluno.
8. Escolhe produtos.
9. Cria o pedido.
10. Sistema calcula corretamente o valor.
11. Cobrança Pix real é criada.
12. Responsável realiza o pagamento.
13. Provedor confirma o pagamento.
14. Zelloa processa a confirmação.
15. Pedido muda para pago.
16. Cantina visualiza o pedido.
17. Funcionário inicia preparação.
18. Marca como pronto.
19. Marca como entregue.
20. Responsável consegue consultar o pedido no histórico.

O fluxo deverá funcionar **sem envio ou conferência manual de comprovante Pix**.

---

# 32. Critérios de Segurança Financeira

Antes da entrada em produção, deverão existir testes comprovando que:

- alterar valores pelo frontend não altera o valor cobrado;
- webhook duplicado não duplica processamento;
- webhook inválido não confirma pagamento;
- usuário de outra instituição não acessa pedidos;
- responsável não acessa aluno não vinculado;
- funcionário não altera operações não autorizadas;
- cobrança expirada é tratada corretamente;
- pagamento confirmado é associado ao pedido correto;
- falhas temporárias do provedor não causam perda do pedido.

---

# 33. Objetivo do Projeto-Piloto

A primeira implantação deverá validar:

### Segurança

Redução ou eliminação da liberação de pedidos baseada exclusivamente em comprovantes.

### Operação

Redução do trabalho manual da cantina.

### Experiência

Facilidade de utilização pelos responsáveis.

### Adoção

Quantidade de responsáveis que conseguem concluir pedidos sem suporte.

### Viabilidade comercial

Interesse da instituição em continuar utilizando a solução após o período de validação.

---

# 34. Evolução do Produto

A Zelloa não deverá ser posicionada exclusivamente como sistema de cantina.

A alimentação escolar será o primeiro caso de uso.

A visão futura poderá envolver outros serviços oferecidos pelas instituições aos responsáveis.

Possíveis contextos futuros incluem:

- eventos;
- passeios;
- materiais;
- produtos;
- serviços;
- pagamentos específicos;
- comunicação relacionada às operações.

Esses itens representam visão de produto e **não fazem parte do escopo atual**.

---

# 35. Identidade Conceitual

**Produto:** Zelloa

**Origem conceitual:** zelo.

Zelloa representa cuidado com:

- alunos;
- famílias;
- escola;
- tempo;
- operação;
- segurança.

### Posicionamento

> **Zelloa — Tecnologia que simplifica. Cuidado que conecta.**

A marca deverá permanecer independente do primeiro módulo funcional para permitir evolução do produto.

---

# 36. Regra para Alteração deste Documento

Este documento representa a especificação funcional do MVP.

Durante o desenvolvimento:

1. uma nova funcionalidade não deverá ser assumida automaticamente;
2. requisitos ambíguos deverão ser esclarecidos;
3. alterações relevantes deverão ser incorporadas à especificação;
4. decisões técnicas deverão permanecer no documento de arquitetura;
5. funcionalidades futuras não deverão ser antecipadas apenas porque parecem úteis.

A implementação deverá seguir o escopo documentado.

---

# 37. Definition of Done — Produto MVP

O MVP será considerado concluído quando:

- o fluxo principal funcionar ponta a ponta;
- existir integração Pix real;
- pagamentos forem confirmados automaticamente;
- nenhuma confirmação depender de comprovantes enviados;
- responsáveis estiverem isolados pelos seus vínculos;
- instituições estiverem isoladas entre si;
- a cantina possuir fluxo operacional funcional;
- operações críticas forem auditáveis;
- testes dos fluxos financeiros estiverem passando;
- aplicação estiver publicada em ambiente adequado para piloto;
- documentação técnica estiver atualizada.

---

**Fim do documento — Zelloa Product Specification v1.0**
