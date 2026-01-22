import { useState } from 'react';

// Component 1: Input Screen
function InputScreen({ onCompute, onReturn }) {
  const [arg1, setArg1] = useState('');
  const [arg2, setArg2] = useState('');

  const handleCompute = () => {
    const num1 = parseFloat(arg1) || 0;
    const num2 = parseFloat(arg2) || 0;
    onCompute(num1, num2);
  };

  return (
    <div className="min-h-screen bg-gradient-to-br from-blue-50 to-indigo-100 flex items-center justify-center p-4">
      <div className="bg-white rounded-lg shadow-xl p-8 w-full max-w-md">
        <h1 className="text-3xl font-bold text-gray-800 mb-6 text-center">
          Calculator
        </h1>

        <div className="space-y-4">
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-2">
              Number 1:
            </label>
            <input
              type="number"
              value={arg1}
              onChange={(e) => setArg1(e.target.value)}
              placeholder="Enter arg1"
              className="w-full px-4 py-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-indigo-500 focus:border-transparent text-lg"
            />
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-2">
              Number 2:
            </label>
            <input
              type="number"
              value={arg2}
              onChange={(e) => setArg2(e.target.value)}
              placeholder="Enter arg2"
              className="w-full px-4 py-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-indigo-500 focus:border-transparent text-lg"
            />
          </div>

          <div className="flex gap-3 pt-4">
            <button
              onClick={handleCompute}
              className="flex-1 bg-green-600 text-white py-3 px-6 rounded-lg hover:bg-green-700 transition-colors font-semibold text-lg"
            >
              Compute
            </button>
            <button
              onClick={onReturn}
              className="flex-1 bg-blue-600 text-white py-3 px-6 rounded-lg hover:bg-blue-700 transition-colors font-semibold text-lg"
            >
              Return
            </button>
          </div>
        </div>
      </div>
    </div>
  );
}

// Component 2: Result Screen
function ResultScreen({ result, onReturn }) {
  return (
    <div className="min-h-screen bg-gradient-to-br from-green-50 to-emerald-100 flex items-center justify-center p-4">
      <div className="bg-white rounded-lg shadow-xl p-8 w-full max-w-md">
        <h1 className="text-3xl font-bold text-gray-800 mb-6 text-center">
          Calculation Result
        </h1>

        <div className="bg-green-50 border-2 border-green-500 rounded-lg p-6 mb-6">
          <div className="text-center">
            <p className="text-lg text-gray-600 mb-2">Result =</p>
            <p className="text-5xl font-bold text-green-600">{result}</p>
          </div>
        </div>

        <button
          onClick={onReturn}
          className="w-full bg-blue-600 text-white py-3 px-6 rounded-lg hover:bg-blue-700 transition-colors font-semibold text-lg"
        >
          Return
        </button>
      </div>
    </div>
  );
}

// Component 3: Main App (State Management)
export default function App() {
  const [currentScreen, setCurrentScreen] = useState('input');
  const [result, setResult] = useState(0);

  const handleCompute = (num1, num2) => {
    const sum = num1 + num2;
    setResult(sum);
    setCurrentScreen('result');
  };

  const handleReturn = () => {
    setCurrentScreen('input');
  };

  return (
    <>
      {currentScreen === 'input' && (
        <InputScreen
          onCompute={handleCompute}
          onReturn={handleReturn}
        />
      )}

      {currentScreen === 'result' && (
        <ResultScreen
          result={result}
          onReturn={handleReturn}
        />
      )}
    </>
  );
}