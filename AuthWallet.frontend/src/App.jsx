import { useState, useEffect } from "react";
import { BrowserRouter, Routes, Route, Navigate } from "react-router-dom";
import { AuthProvider } from "./context/AuthContext";
import ProtectedRoute from "./components/ProtectedRoute";
import Navbar from "./components/Navbar";
import HomePage from "./pages/HomePage";
import TransferPage from "./pages/TransferPage";
import LoginPage from "./pages/LoginPage";
import RegisterPage from "./pages/RegisterPage";
import "./App.css";

function AppContent() {
  const [theme, setTheme] = useState("auto");

  // Auto Dark Mode logic based on time (>= 18:00 or < 06:00)
  useEffect(() => {
    const applyTheme = () => {
      const currentHour = new Date().getHours();
      const isNight = currentHour >= 18 || currentHour < 6;
      const effectiveTheme = isNight ? "dark" : "light";
      document.documentElement.setAttribute("data-theme", effectiveTheme);
    };

    applyTheme();
    const interval = setInterval(applyTheme, 60000); // check every minute
    return () => clearInterval(interval);
  }, []);

  return (
    <div className="app-shell">
      <Navbar />
      <main className="main-content">
        <Routes>
          <Route
            path="/"
            element={
              <ProtectedRoute>
                <HomePage />
              </ProtectedRoute>
            }
          />
          <Route
            path="/transfer"
            element={
              <ProtectedRoute>
                <TransferPage />
              </ProtectedRoute>
            }
          />
          <Route path="/login" element={<LoginPage />} />
          <Route path="/register" element={<RegisterPage />} />
          <Route path="*" element={<Navigate to="/" replace />} />
        </Routes>
      </main>
    </div>
  );
}

export default function App() {
  return (
    <BrowserRouter>
      <AuthProvider>
        <AppContent />
      </AuthProvider>
    </BrowserRouter>
  );
}
