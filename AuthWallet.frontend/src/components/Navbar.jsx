import { Link, useLocation } from "react-router-dom";
import { useAuth } from "../context/AuthContext";

export default function Navbar() {
  const { user, logout } = useAuth();
  const location = useLocation();

  if (!user) return null;

  return (
    <nav className="navbar">
      <div className="navbar-brand">
        <span className="navbar-logo">W</span>
        <span>WalletAuth</span>
      </div>
      <div className="navbar-links">
        <Link to="/" className={`nav-link ${location.pathname === "/" ? "active" : ""}`}>
          Home
        </Link>
        <Link to="/transfer" className={`nav-link ${location.pathname === "/transfer" ? "active" : ""}`}>
          Transfer
        </Link>
      </div>
      <div className="navbar-user">
        <span className="user-email">{user.email}</span>
        <button onClick={logout} className="btn btn-outline btn-sm">Logout</button>
      </div>
    </nav>
  );
}
