import { useState } from 'react';

function InputScreen({ onCompute, onReturn }) {
  const [arg1, setArg1] = useState('');
  const [arg2, setArg2] = useState('');

  return (
    <div>
      <input value={arg1} onChange={(e) => setArg1(e.target.value)} />
      <input value={arg2} onChange={(e) => setArg2(e.target.value)} />
      <button onClick={() => onCompute(Number(arg1), Number(arg2))}>
        Compute
      </button>
      <button onClick={onReturn}>Return</button>
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

  const compute = (n1, n2) => {
    setResult(n1 + n2);
    setScreen('result');
  };

  return screen === 'input' 
    ? <InputScreen onCompute={compute} onReturn={() => setScreen('input')} />
    : <ResultScreen result={result} onReturn={() => setScreen('input')} />;
}