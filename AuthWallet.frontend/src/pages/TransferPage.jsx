import { useState, useEffect } from "react";
import client from "../api/client";

export default function TransferPage() {
  const [recipientEmail, setRecipientEmail] = useState("");
  const [amount, setAmount] = useState("");
  const [notes, setNotes] = useState("");
  const [wallet, setWallet] = useState(null);
  const [loading, setLoading] = useState(false);
  const [fetchLoading, setFetchLoading] = useState(true);
  const [error, setError] = useState("");
  const [success, setSuccess] = useState(null);

  // Generate a random UUID v4
  const generateUUID = () => {
    if (typeof crypto !== "undefined" && crypto.randomUUID) {
      return crypto.randomUUID();
    }
    return "xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx".replace(/[xy]/g, (c) => {
      const r = (Math.random() * 16) | 0;
      const v = c === "x" ? r : (r & 0x3) | 0x8;
      return v.toString(16);
    });
  };

  const [idempotencyKey, setIdempotencyKey] = useState(generateUUID());

  const fetchWallet = async () => {
    setFetchLoading(true);
    try {
      const { data } = await client.get("/wallet");
      setWallet(data);
    } catch (err) {
      setError(
        err.response?.data?.error ||
        err.message ||
        "Failed to load wallet information"
      );
    } finally {
      setFetchLoading(false);
    }
  };

  useEffect(() => {
    fetchWallet();
  }, []);

  const handleTransfer = async (e) => {
    e.preventDefault();
    setError("");
    setSuccess(null);

    const parsedAmount = parseFloat(amount);
    if (isNaN(parsedAmount) || parsedAmount <= 0) {
      setError("Please enter a valid amount greater than 0");
      return;
    }

    if (!recipientEmail) {
      setError("Please enter recipient email");
      return;
    }

    if (wallet && parsedAmount > wallet.balance) {
      setError("Insufficient balance in your wallet");
      return;
    }

    setLoading(true);
    try {
      const payload = {
        recipientEmail: recipientEmail.trim(),
        amount: parsedAmount,
        notes: notes.trim() || null,
        idempotencyKey: idempotencyKey,
      };

      const { data } = await client.post("/wallet/transfer", payload);

      setSuccess({
        recipient: data.recipientEmail,
        amount: data.amount,
        transactionId: data.transactionId,
      });

      // Clear inputs and generate a new idempotency key for the next transfer
      setRecipientEmail("");
      setAmount("");
      setNotes("");
      setIdempotencyKey(generateUUID());

      // Refresh wallet & history
      await fetchWallet();
    } catch (err) {
      const msg =
        err.response?.data?.error ||
        err.response?.data?.title ||
        err.message ||
        "Transfer failed. Please check connection and try again.";
      setError(msg);
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="page-container">
      <header className="page-header">
        <h1>Transfer Funds</h1>
        <p className="page-subtitle">
          Send money securely to any registered account. Transactions are idempotent for reliability in unstable networks.
        </p>
      </header>

      <div className="dashboard-grid">
        {/* Transfer Form Card */}
        <div className="card">
          <div className="card-header">
            <h3>New Transfer</h3>
            <span className="balance-pill">
              Available: Rp
              {wallet?.balance?.toLocaleString(undefined, {
                minimumFractionDigits: 2,
                maximumFractionDigits: 2,
              }) ?? "0.00"}
            </span>
          </div>

          {error && <div className="alert alert-error">{error}</div>}

          {success && (
            <div className="alert alert-success">
              <strong>Transfer Successful!</strong>
              <p>
                Sent Rp{success.amount.toFixed(2)} to {success.recipient}.
              </p>
              <small className="font-mono text-xs">TX: {success.transactionId}</small>
            </div>
          )}

          <form onSubmit={handleTransfer} className="transfer-form">
            <div className="form-group">
              <label htmlFor="recipient">Recipient Email</label>
              <input
                id="recipient"
                type="email"
                placeholder="recipient@example.com"
                value={recipientEmail}
                onChange={(e) => setRecipientEmail(e.target.value)}
                required
                disabled={loading}
              />
            </div>

            <div className="form-group">
              <label htmlFor="amount">Amount ($)</label>
              <input
                id="amount"
                type="number"
                step="0.01"
                min="0.01"
                placeholder="0.00"
                value={amount}
                onChange={(e) => setAmount(e.target.value)}
                required
                disabled={loading}
              />
            </div>

            <div className="form-group">
              <label htmlFor="notes">Notes / Memo (Optional)</label>
              <input
                id="notes"
                type="text"
                placeholder="e.g. Dinner reimbursement, Jungle supplies"
                value={notes}
                onChange={(e) => setNotes(e.target.value)}
                disabled={loading}
                maxLength={500}
              />
            </div>

            <div className="idempotency-info text-xs text-muted">
              <span>Idempotency Key: </span>
              <code className="font-mono">{idempotencyKey.slice(0, 13)}...</code>
              <span title="Protects against double payments if network drops">(Protected)</span>
            </div>

            <button type="submit" className="btn btn-primary btn-block" disabled={loading}>
              {loading ? "Processing Transfer..." : "Confirm & Send Transfer"}
            </button>
          </form>
        </div>

        {/* Transaction History Card */}
        <div className="card">
          <div className="card-header">
            <h3>Transaction Ledger</h3>
            <button
              onClick={fetchWallet}
              className="btn btn-sm btn-outline"
              disabled={fetchLoading}
            >
              {fetchLoading ? "Refreshing..." : "Refresh"}
            </button>
          </div>

          {fetchLoading && !wallet ? (
            <div className="loading-state">
              <div className="spinner"></div>
              <p>Loading ledger records...</p>
            </div>
          ) : !wallet?.recentTransactions || wallet.recentTransactions.length === 0 ? (
            <div className="empty-state">
              <p>No transaction history found.</p>
            </div>
          ) : (
            <div className="table-responsive">
              <table className="data-table">
                <thead>
                  <tr>
                    <th>Type</th>
                    <th>Counterparty</th>
                    <th>Notes</th>
                    <th>Time</th>
                    <th className="text-right">Amount</th>
                  </tr>
                </thead>
                <tbody>
                  {wallet.recentTransactions.map((tx) => (
                    <tr key={tx.id}>
                      <td>
                        <span
                          className={`badge ${
                            tx.direction === "received"
                              ? "badge-success"
                              : "badge-neutral"
                          }`}
                        >
                          {tx.direction === "received" ? "In" : "Out"}
                        </span>
                      </td>
                      <td className="font-mono text-sm" title={tx.counterPartyEmail}>
                        {tx.counterPartyEmail}
                      </td>
                      <td className="text-sm">{tx.notes || "—"}</td>
                      <td className="text-muted text-xs">
                        {new Date(tx.createdAt).toLocaleTimeString([], {
                          hour: "2-digit",
                          minute: "2-digit",
                          month: "short",
                          day: "numeric",
                        })}
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
    </div>
  );
}
