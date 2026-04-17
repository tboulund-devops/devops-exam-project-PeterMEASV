import { useEffect, useState } from "react";
import { useAtomValue } from "jotai";
import { useNavigate } from "react-router";
import { userInfoAtom } from "./Token.tsx";
import { userClient } from "./baseUrl.ts";
import type { User } from "./generated-ts-client.ts";

function FriendSidebar() {
    const navigate = useNavigate();
    const userInfo = useAtomValue(userInfoAtom);
    const userId = userInfo?.id;

    const [friends, setFriends] = useState<User[]>([]);
    const [friendIdInput, setFriendIdInput] = useState("");
    const [error, setError] = useState<string | null>(null);
    const [adding, setAdding] = useState(false);

    useEffect(() => {
        if (!userId) return;
        userClient.getAllFriendsForUser(userId)
            .then(setFriends)
            .catch(() => setError("Could not load friends."));
    }, [userId]);

    const handleAdd = async (e: React.FormEvent) => {
        e.preventDefault();
        if (!userId || !friendIdInput.trim()) return;
        setAdding(true);
        setError(null);
        try {
            await userClient.addFriendForUser(userId, friendIdInput.trim());
            const updated = await userClient.getAllFriendsForUser(userId);
            setFriends(updated);
            setFriendIdInput("");
        } catch {
            setError("Could not add friend.");
        } finally {
            setAdding(false);
        }
    };

    const handleRemove = async (friendId: string, e: React.MouseEvent) => {
        e.stopPropagation(); // Prevent navigation when clicking remove
        if (!userId) return;
        try {
            await userClient.removeFriendForUser(userId, friendId);
            setFriends(prev => prev.filter(f => f.id !== friendId));
        } catch {
            setError("Could not remove friend.");
        }
    };

    const handleFriendClick = (friendId: string) => {
        navigate(`/friend/${friendId}`);
    };

    return (
        <aside style={{
            width: "220px",
            height: "100vh",
            position: "sticky",
            top: 0,
            background: "#111",
            borderRight: "1px solid #222",
            display: "flex",
            flexDirection: "column",
            padding: "20px 14px",
            gap: "16px",
            flexShrink: 0,
            overflowY: "auto",
        }}>
            <h3 style={{ margin: 0, fontSize: "1rem", color: "#fff" }}>Friends</h3>

            <form onSubmit={handleAdd} style={{ display: "flex", flexDirection: "column", gap: "8px" }}>
                <input
                    placeholder="Friend's user ID"
                    value={friendIdInput}
                    onChange={e => setFriendIdInput(e.target.value)}
                    style={{
                        padding: "6px 10px",
                        borderRadius: "6px",
                        border: "1px solid #333",
                        background: "#1a1a1a",
                        color: "#fff",
                        fontSize: "0.85rem",
                        width: "100%",
                        boxSizing: "border-box",
                    }}
                />
                <button
                    type="submit"
                    disabled={adding || !friendIdInput.trim()}
                    style={{
                        padding: "6px",
                        borderRadius: "6px",
                        background: "#e50914",
                        color: "#fff",
                        border: "none",
                        cursor: "pointer",
                        fontSize: "0.85rem",
                        fontWeight: "bold",
                    }}
                >
                    {adding ? "Adding..." : "Add friend"}
                </button>
            </form>

            {error && <p style={{ color: "red", fontSize: "0.8rem", margin: 0 }}>{error}</p>}

            <ul style={{ listStyle: "none", margin: 0, padding: 0, display: "flex", flexDirection: "column", gap: "8px" }}>
                {friends.length === 0 && (
                    <li style={{ color: "#555", fontSize: "0.85rem" }}>No friends yet.</li>
                )}
                {friends.map(friend => (
                    <li 
                        key={friend.id} 
                        onClick={() => handleFriendClick(friend.id!)}
                        style={{
                            display: "flex",
                            alignItems: "center",
                            justifyContent: "space-between",
                            gap: "6px",
                            cursor: "pointer",
                            padding: "8px",
                            borderRadius: "6px",
                            background: "#1a1a1a",
                            transition: "background 0.2s ease",
                        }}
                        onMouseEnter={(e) => e.currentTarget.style.background = "#2a2a2a"}
                        onMouseLeave={(e) => e.currentTarget.style.background = "#1a1a1a"}
                    >
                        <span style={{ fontSize: "0.85rem", color: "#ddd", overflow: "hidden", textOverflow: "ellipsis", whiteSpace: "nowrap" }}>
                            {friend.name ?? friend.email ?? friend.id}
                        </span>
                        <button
                            onClick={(e) => handleRemove(friend.id!, e)}
                            title="Remove friend"
                            style={{
                                background: "none",
                                border: "none",
                                color: "#555",
                                cursor: "pointer",
                                fontSize: "1rem",
                                lineHeight: 1,
                                flexShrink: 0,
                            }}
                        >
                            ✕
                        </button>
                    </li>
                ))}
            </ul>
        </aside>
    );
}

export default FriendSidebar;
