/**
 * PÁGINA DE TESTE MÍNIMA - SEM PROVIDERS
 * Para debuggar travamento crítico
 */

'use client';

export default function TestPage() {
  return (
    <div style={{ padding: '20px', fontFamily: 'Arial' }}>
      <h1>🧪 TESTE MÍNIMO - SEM PROVIDERS</h1>

      <div style={{ marginBottom: '20px' }}>
        <p>Se esta página NÃO travar ao clicar, o problema estava nos providers.</p>
      </div>

      <div style={{ marginBottom: '20px' }}>
        <button
          onClick={() => console.log('Botão clicado!')}
          style={{
            padding: '10px 20px',
            backgroundColor: '#4CAF50',
            color: 'white',
            border: 'none',
            borderRadius: '4px',
            cursor: 'pointer'
          }}
        >
          Clique Aqui Para Testar
        </button>
      </div>

      <div style={{ marginBottom: '20px' }}>
        <input
          type="text"
          placeholder="Digite aqui para testar"
          style={{
            padding: '10px',
            borderRadius: '4px',
            border: '1px solid #ccc',
            width: '200px'
          }}
        />
      </div>

      <div>
        <p>✅ Se funcionou = problema estava nos providers (RESOLVIDO)</p>
        <p>❌ Se travou = problema mais profundo no Next.js/React</p>
      </div>
    </div>
  );
}