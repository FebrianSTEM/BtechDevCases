import { createContext, useContext, useState, useEffect, useCallback, useRef } from "react";
import { useNavigate } from "react-router-dom";
import client from "../api/client";

const AuthContext = createContext(null);

export function AuthProvider({ children }) {
  const [user, setUser] = useState(null);
  const [loading, setLoading] = useState(true);
  const lastActivityRef = useRef(Date.now());
  const navigate = useNavigate();
  // Decode JWT to extract email
  const decodeToken = (token) => {
    try {
      const payload = JSON.parse(atob(token.split(".")[1]));
      return { email: payload.email, 
               userId: payload.sub,
               inactiveMinutes: payload.inactiveMinutes
             };
    } catch {
      return null;
    }
  };
  const INACTIVITY_TIMEOUT = Number(user?.inactiveMinutes ?? 15) * 60 * 1000; // 1 minutes in ms


  // Initialize from localStorage
  useEffect(() => {
    const token = localStorage.getItem("accessToken");
    if (token) {
      const decoded = decodeToken(token);
      if (decoded) setUser(decoded);
    }
    setLoading(false);
  }, []);

  // Inactivity tracker
  useEffect(() => {
    if (!user) return;

    const updateActivity = () => {
      lastActivityRef.current = Date.now();
    };

    const checkInactivity = () => {
      if (Date.now() - lastActivityRef.current > INACTIVITY_TIMEOUT) {
        logout();
      }
    };

    window.addEventListener("click", updateActivity);
    window.addEventListener("keypress", updateActivity);
    window.addEventListener("mousemove", updateActivity);
    const interval = setInterval(checkInactivity, 60000); // Check every minute

    return () => {
      window.removeEventListener("click", updateActivity);
      window.removeEventListener("keypress", updateActivity);
      window.removeEventListener("mousemove", updateActivity);
      clearInterval(interval);
    };
  }, [user]);

  const login = useCallback(async (email, password) => {
    const { data } = await client.post("/auth/login", { email, password });
    localStorage.setItem("accessToken", data.accessToken);
    localStorage.setItem("refreshToken", data.refreshToken);
    const decoded = decodeToken(data.accessToken);
    setUser(decoded);
    lastActivityRef.current = Date.now();
    return data;
  }, []);

  const register = useCallback(async (email, password, confirmPassword) => {
    const { data } = await client.post("/auth/register", { email, password, confirmPassword });
    localStorage.setItem("accessToken", data.accessToken);
    localStorage.setItem("refreshToken", data.refreshToken);
    const decoded = decodeToken(data.accessToken);
    setUser(decoded);
    lastActivityRef.current = Date.now();
    return data;
  }, []);

  const logout = useCallback(async () => {
    try {
      const refreshToken = localStorage.getItem("refreshToken");
      if (refreshToken) {
        await client.post("/auth/logout", { refreshToken });
      }
    } catch { /* ignore errors during logout */ }
    localStorage.clear();
    setUser(null);
    navigate("/login");
  }, [navigate]);

  if (loading) return null;

  return (
    <AuthContext.Provider value={{ user, login, register, logout }}>
      {children}
    </AuthContext.Provider>
  );
}

export function useAuth() {
  const context = useContext(AuthContext);
  if (!context) throw new Error("useAuth must be used within AuthProvider");
  return context;
}
