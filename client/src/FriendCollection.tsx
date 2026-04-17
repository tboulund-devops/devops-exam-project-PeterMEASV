import { useEffect, useState } from "react";
import { useNavigate, useParams } from "react-router";
import { useAtomValue } from "jotai";
import { movieClient, userClient } from "./baseUrl.ts";
import type { Movie, User } from "./generated-ts-client.ts";
import { userInfoAtom } from "./Token.tsx";

type MovieWithSeen = Movie & { seen?: boolean | null; rating?: number | null };

type TabType = "watchlist" | "seen";

function FriendCollection() {
    const { friendId } = useParams<{ friendId: string }>();
    const navigate = useNavigate();
    const userInfo = useAtomValue(userInfoAtom);
    const userId = userInfo?.id;
    
    const [movies, setMovies] = useState<MovieWithSeen[]>([]);
    const [friendName, setFriendName] = useState<string>("");
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);
    const [activeTab, setActiveTab] = useState<TabType>("watchlist");
    const [myMovieIds, setMyMovieIds] = useState<Set<string>>(new Set());

    useEffect(() => {
        if (!friendId || !userId) return;

        const fetchFriendData = async () => {
            setLoading(true);
            try {
                const [moviesData, friends, myMovies] = await Promise.all([
                    movieClient.getMoviesByUser(friendId),
                    userClient.getAllFriendsForUser(userId),
                    movieClient.getMoviesByUser(userId),
                ]);

                setMovies(moviesData as MovieWithSeen[]);
                setMyMovieIds(new Set(myMovies.map(m => m.id!).filter(Boolean)));

                const friend = friends.find((f: User) => f.id === friendId);
                setFriendName(friend ? (friend.name ?? friend.email ?? friendId) : friendId);
            } catch {
                setError("Could not load friend's collection.");
            } finally {
                setLoading(false);
            }
        };

        fetchFriendData();
    }, [friendId, userId]);

    const seenMovies = movies.filter(m => m.seen === true);
    const watchlistMovies = movies.filter(m => m.seen !== true);

    return (
        <div style={{ minHeight: "100vh", background: "#111", color: "#fff" }}>
            {/* Header with tabs */}
            <div style={{
                position: "sticky",
                top: 0,
                background: "#1a1a1a",
                borderBottom: "1px solid #333",
                padding: "16px 24px",
                zIndex: 10,
            }}>
                <button
                    onClick={() => navigate("/dashboard")}
                    style={{
                        ...secondaryButtonStyle,
                        marginBottom: "12px",
                    }}
                >
                    ← Back to My Collection
                </button>
                <div style={{
                    display: "flex",
                    gap: "24px",
                    alignItems: "center",
                }}>
                    <h1 style={{ margin: 0, fontSize: "1.5rem" }}>
                        {friendName ? `${friendName}'s Collection` : "Friend's Collection"}
                    </h1>
                    <div style={{ display: "flex", gap: "8px" }}>
                        <button
                            onClick={() => setActiveTab("watchlist")}
                            style={{
                                ...tabButtonStyle,
                                background: activeTab === "watchlist" ? "#e50914" : "transparent",
                                color: activeTab === "watchlist" ? "#fff" : "#aaa",
                            }}
                        >
                            Watchlist ({watchlistMovies.length})
                        </button>
                        <button
                            onClick={() => setActiveTab("seen")}
                            style={{
                                ...tabButtonStyle,
                                background: activeTab === "seen" ? "#e50914" : "transparent",
                                color: activeTab === "seen" ? "#fff" : "#aaa",
                            }}
                        >
                            Watched ({seenMovies.length})
                        </button>
                    </div>
                </div>
            </div>

            {/* Content */}
            <div style={{ padding: "24px" }}>
                {loading && <p>Loading...</p>}
                {!loading && error && <p style={{ color: "red" }}>{error}</p>}

                {!loading && !error && (
                    <>
                        {/* Watchlist Tab */}
                        {activeTab === "watchlist" && (
                            <>
                                {watchlistMovies.length === 0 ? (
                                    <div style={{
                                        textAlign: "center",
                                        padding: "80px 20px",
                                        color: "#666",
                                    }}>
                                        <div style={{ fontSize: "4rem", marginBottom: "16px" }}>🎬</div>
                                        <h2 style={{ margin: "0 0 8px 0", color: "#aaa" }}>
                                            {friendName ? `${friendName}'s watchlist is empty` : "Watchlist is empty"}
                                        </h2>
                                    </div>
                                ) : (
                                    <div style={{
                                        display: "grid",
                                        gridTemplateColumns: "repeat(auto-fill, minmax(180px, 1fr))",
                                        gap: "20px",
                                    }}>
                                        {watchlistMovies.map(movie => (
                                            <FriendMovieCard
                                                key={movie.id}
                                                movie={movie}
                                                userId={userId}
                                                myMovieIds={myMovieIds}
                                                onAdded={id => setMyMovieIds(prev => new Set([...prev, id]))}
                                            />
                                        ))}
                                    </div>
                                )}
                            </>
                        )}

                        {/* Seen Tab */}
                        {activeTab === "seen" && (
                            <>
                                {seenMovies.length === 0 ? (
                                    <div style={{
                                        textAlign: "center",
                                        padding: "80px 20px",
                                        color: "#666",
                                    }}>
                                        <div style={{ fontSize: "4rem", marginBottom: "16px" }}>✓</div>
                                        <h2 style={{ margin: "0 0 8px 0", color: "#aaa" }}>No movies watched yet</h2>
                                    </div>
                                ) : (
                                    <div style={{
                                        display: "grid",
                                        gridTemplateColumns: "repeat(auto-fill, minmax(180px, 1fr))",
                                        gap: "20px",
                                    }}>
                                        {seenMovies.map(movie => (
                                            <FriendMovieCard
                                                key={movie.id}
                                                movie={movie}
                                                showSeenBadge
                                                userId={userId}
                                                myMovieIds={myMovieIds}
                                                onAdded={id => setMyMovieIds(prev => new Set([...prev, id]))}
                                            />
                                        ))}
                                    </div>
                                )}
                            </>
                        )}
                    </>
                )}
            </div>
        </div>
    );
}

// ── Friend Movie Card Component ──────────────────────────────────────────────

type FriendMovieCardProps = {
    movie: MovieWithSeen;
    showSeenBadge?: boolean;
    userId?: string;
    myMovieIds?: Set<string>;
    onAdded?: (movieId: string) => void;
};

function FriendMovieCard({ movie, showSeenBadge, userId, myMovieIds, onAdded }: FriendMovieCardProps) {
    const [adding, setAdding] = useState(false);
    const alreadyOwned = movie.id ? myMovieIds?.has(movie.id) : true;

    const handleAdd = async (e: React.MouseEvent) => {
        e.stopPropagation();
        if (!userId || !movie.id || alreadyOwned) return;
        setAdding(true);
        try {
            await movieClient.addMovieToUser(movie.id, userId);
            onAdded?.(movie.id);
        } finally {
            setAdding(false);
        }
    };

    return (
        <div
            style={{
                position: "relative",
                display: "flex",
                flexDirection: "column",
                gap: "8px",
                transition: "transform 0.2s ease",
            }}
            onMouseEnter={(e) => e.currentTarget.style.transform = "scale(1.05)"}
            onMouseLeave={(e) => e.currentTarget.style.transform = "scale(1)"}
        >
            {movie.photo
                ? <img 
                    src={movie.photo} 
                    alt={movie.title} 
                    style={{ 
                        width: "100%", 
                        aspectRatio: "2/3", 
                        objectFit: "cover", 
                        borderRadius: "6px",
                        boxShadow: "0 4px 12px rgba(0,0,0,0.3)",
                    }} 
                  />
                : <div style={{ 
                    width: "100%", 
                    aspectRatio: "2/3", 
                    background: "#2a2a2a", 
                    borderRadius: "6px", 
                    display: "flex", 
                    alignItems: "center", 
                    justifyContent: "center", 
                    color: "#666" 
                  }}>
                    No poster
                  </div>
            }
            {showSeenBadge && (
                <div style={{
                    position: "absolute",
                    top: "8px",
                    right: "8px",
                    background: "#4caf50",
                    color: "#fff",
                    borderRadius: "50%",
                    width: "32px",
                    height: "32px",
                    display: "flex",
                    alignItems: "center",
                    justifyContent: "center",
                    fontWeight: "bold",
                    fontSize: "18px",
                    boxShadow: "0 2px 8px rgba(0,0,0,0.4)",
                }}>
                    ✓
                </div>
            )}
            {movie.rating && movie.rating > 0 && (
                <div style={{
                    position: "absolute",
                    top: "8px",
                    left: "8px",
                    background: "rgba(255, 193, 7, 0.95)",
                    color: "#000",
                    borderRadius: "6px",
                    padding: "4px 8px",
                    display: "flex",
                    alignItems: "center",
                    justifyContent: "center",
                    fontWeight: "bold",
                    fontSize: "14px",
                    boxShadow: "0 2px 8px rgba(0,0,0,0.4)",
                    gap: "4px",
                }}>
                    <span>⭐</span>
                    <span>{movie.rating}</span>
                </div>
            )}
            <div style={{ display: "flex", alignItems: "flex-start", justifyContent: "space-between", gap: "4px" }}>
                <div style={{ display: "flex", flexDirection: "column", gap: "4px", minWidth: 0 }}>
                    <span style={{ fontWeight: "bold", fontSize: "0.9rem" }}>{movie.title}</span>
                    <span style={{ fontSize: "0.8rem", color: "#aaa" }}>{movie.year}</span>
                </div>
                {!alreadyOwned && (
                    <button
                        onClick={handleAdd}
                        disabled={adding}
                        title="Add to my watchlist"
                        style={{
                            flexShrink: 0,
                            background: "none",
                            border: "none",
                            color: "#aaa",
                            cursor: adding ? "default" : "pointer",
                            fontSize: "22px",
                            lineHeight: 1,
                            padding: "0 2px",
                            transition: "color 0.15s ease, transform 0.15s ease",
                        }}
                        onMouseEnter={e => { e.currentTarget.style.color = "#fff"; e.currentTarget.style.transform = "scale(1.3)"; }}
                        onMouseLeave={e => { e.currentTarget.style.color = "#aaa"; e.currentTarget.style.transform = "scale(1)"; }}
                    >
                        {adding ? "…" : "+"}
                    </button>
                )}
            </div>
        </div>
    );
}

// ── Styles ────────────────────────────────────────────────────────────────────

const secondaryButtonStyle: React.CSSProperties = {
    padding: "8px 16px",
    borderRadius: "6px",
    background: "#2a2a2a",
    color: "#fff",
    border: "none",
    cursor: "pointer",
};

const tabButtonStyle: React.CSSProperties = {
    padding: "8px 20px",
    borderRadius: "6px",
    border: "none",
    cursor: "pointer",
    fontWeight: "bold",
    fontSize: "0.95rem",
    transition: "all 0.2s ease",
};

export default FriendCollection;
