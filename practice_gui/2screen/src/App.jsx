import { useState } from 'react';

function InputScreen({ onCompute, onReturn }) {
  const [a, setA] = useState('');
  const [b, setB] = useState('');
  
  return (
    <div>
      <input value={a} onChange={e => setA(e.target.value)} />
      <input value={b} onChange={e => setB(e.target.value)} />
      <button onClick={() => onCompute(+a, +b)}>Compute</button>
      <button onClick={onReturn}>Return</button>r
    </div>
  );
}

function ResultScreen({ result, onReturn }) {
  return (
    <div>
      <p>Result = {result}</p>
      <button onClick={onReturn}>Return</button>
    </div>
  );
}

export default function App() {
  const [screen, setScreen] = useState('input');
  const [result, setResult] = useState(0);

  return screen === 'input' 
    ? <InputScreen 
        onCompute={(n1, n2) => {
          setResult(n1 + n2);
          setScreen('result');
        }} 
        onReturn={() => setScreen('input')} 
      />
    : <ResultScreen result={result} onReturn={() => setScreen('input')} />;
}