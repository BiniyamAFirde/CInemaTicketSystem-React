import { useState } from 'react';

export default function Calculator() {
  const [v1, setV1] = useState('');
  const [v2, setV2] = useState('');
  const [result, setResult] = useState('');
  const [history, setHistory] = useState([]);

  const calc = () => {
    const n1 = parseFloat(v1) || 0;
    const n2 = parseFloat(v2) || 0;
    const sum = n1 + n2;
    setResult(sum);
    setHistory([...history, { v1: n1, v2: n2, result: sum }]);
  };

  const undo = () => {
    if (history.length === 0) return;
    const last = history[history.length - 1];
    setV1(last.v1.toString());
    setV2(last.v2.toString());
    setResult('');
    setHistory(history.slice(0, -1));
  };

  return (
    <div>
      <input value={v1} onChange={e => setV1(e.target.value)} />
      <input value={v2} onChange={e => setV2(e.target.value)} />
      <input value={result} readOnly />
      <button onClick={calc}>Calculate</button>
      <button onClick={undo}>Undo</button>
      {history.length > 0 && (
        <div>
          {history.map((h, i) => (
            <div key={i}>{h.v1} + {h.v2} = {h.result}</div>
          ))}
        </div>
      )}
    </div>
  );
}