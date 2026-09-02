import { useState, useEffect } from "react";
import { Link } from "react-router-dom";
import client from "../api/client";
import { useAuth } from "../context/AuthContext";


export default function HomePage() {
  const { user, logout } = useAuth();
  const [welcomeMessage, setWelcomeMessage] = useState("");
  const [wallet, setWallet] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const inactivityMinutes = Number(user?.inactiveMinutes ?? 15);
  const INACTIVITY_TIMEOUT = inactivityMinutes * 60;
  const [countdown, setCountdown] = useState(INACTIVITY_TIMEOUT);

  const fetchData = async () => {
    setLoading(true);
    setError("");
    try {
      const [meRes, walletRes] = await Promise.all([
        client.get("/me"),
        client.get("/wallet"),
      ]);
      setWelcomeMessage(meRes.data.message);
      setWallet(walletRes.data);
    } catch (err) {
      setError(
        err.response?.data?.error ||
        err.message ||
        "Failed to load dashboard data"
      );
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchData();
  }, []);

  useEffect(() => {
  const timer = setInterval(() => {
      setCountdown((prev) => {
        if (prev <= 1) {
          clearInterval(timer);
        
          // Auto Logout
          logout();
          
          return 0;
        }
        return prev - 1;
      });
    }, 1000);

    return () => clearInterval(timer);
  }, []);

  const formatCountdown = (seconds) => {
  const minutes = Math.floor(seconds / 60);
  const remainingSeconds = seconds % 60;

  return `${String(minutes).padStart(2, "0")}:${String(
    remainingSeconds
    ).padStart(2, "0")}`;
  };

  return (
    <div className="page-container">
      <header className="page-header">
        <h1 className="welcome-title">
          {welcomeMessage || `Hello ${user?.email || "User"}, welcome back`}
        </h1>
        <p className="page-subtitle">
          Manage your balance, send transactions, and track account activity.
        </p>
      </header>

      {error && (
        <div className="alert alert-error">
          <span>{error}</span>
          <button onClick={fetchData} className="btn btn-sm btn-outline">
            Retry
          </button>
        </div>
      )}

      {loading ? (
        <div className="loading-state">
          <div className="spinner"></div>
          <p>Loading your wallet information...</p>
        </div>
      ) : (
        <div className="dashboard-grid">
          {/* Balance Card */}
          <div className="card balance-card">
            <div className="card-header">
              <span className="card-tag">Total Balance</span>
              <span className="live-dot" title="Active session"></span>
            </div>
            <div className="balance-amount">
              <span className="currency">Rp</span>
              <span className="amount">
                {wallet?.balance?.toLocaleString(undefined, {
                  minimumFractionDigits: 2,
                  maximumFractionDigits: 2,
                }) ?? "0.00"}
              </span>
            </div>
            <div className="card-actions">
              <Link to="/transfer" className="btn btn-primary">
                Send Money &rarr;
              </Link>
            </div>
          </div>

          {/* Quick Stats / Info Card */}
          <div className="card info-card">
            <h3>Account Overview</h3>
            <div className="info-list">
              <div className="info-item">
                <span className="info-label">Account Email</span>
                <span className="info-value">{user?.email}</span>
              </div>
              <div className="info-item">
                <span className="info-label">Session Inactivity Timeout</span>
                <span className="info-value">{formatCountdown(countdown)}</span>
              </div>
              <div className="info-item">
                <span className="info-label">Recent Transactions</span>
                <span className="info-value">
                  {wallet?.recentTransactions?.length ?? 0} record(s)
                </span>
              </div>
            </div>
          </div>

          {/* Recent Activity Card */}
          <div className="card full-width">
            <div className="card-header">
              <h3>Recent Transactions</h3>
              <Link to="/transfer" className="link text-sm">
                View All / Transfer
              </Link>
            </div>

            {!wallet?.recentTransactions || wallet.recentTransactions.length === 0 ? (
              <div className="empty-state">
                <p>No transactions yet. Send some funds to see history here!</p>
              </div>
            ) : (
              <div className="table-responsive">
                <table className="data-table">
                  <thead>
                    <tr>
                      <th>Type</th>
                      <th>Counterparty</th>
                      <th>Notes</th>
                      <th>Date</th>
                      <th className="text-right">Amount</th>
                    </tr>
                  </thead>
                  <tbody>
                    {wallet.recentTransactions.slice(0, 5).map((tx) => (
                      <tr key={tx.id}>
                        <td>
                          <span
                            className={`badge ${
                              tx.direction === "received"
                                ? "badge-success"
                                : "badge-neutral"
                            }`}
                          >
                            {tx.direction === "received" ? "Received" : "Sent"}
                          </span>
                        </td>
                        <td className="font-mono text-sm">
                          {tx.counterPartyEmail || "-"}
                        </td>
                        <td>{tx.notes || "—"}</td>
                        <td className="text-muted text-sm">
                          {new Date(tx.createdAt).toLocaleString()}
                        </td>
                        <td
                          className={`text-right font-semibold ${
                            tx.direction === "received"
                              ? "text-success"
                              : "text-danger"
                          }`}
                        >
                          {tx.direction === "received" ? "+" : "-"}
                          Rp {tx.amount.toLocaleString("id-ID", {
                          minimumFractionDigits: 2,
                          maximumFractionDigits: 2,
                        })}
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            )}
          </div>
        </div>
      )}
    </div>
  );
}