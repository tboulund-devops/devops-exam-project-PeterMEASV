import { useEffect, useState } from "react";
import { useNavigate, useParams } from "react-router";
import { useAtomValue } from "jotai";
import { genreClient, movieClient, userClient } from "./baseUrl.ts";
import type { Genre, Movie, User } from "./generated-ts-client.ts";
import { userInfoAtom } from "./Token.tsx";

type MovieWithSeen = Movie & { seen?: boolean | null; rating?: number | null; genres?: Genre[] };

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

    const [search, setSearch] = useState("");
    const [sortBy, setSortBy] = useState<"name-asc" | "name-desc" | "rating-asc" | "rating-desc" | "genre" | "">("");
    const [sortOpen, setSortOpen] = useState(false);
    const [selectedFilterGenres, setSelectedFilterGenres] = useState<Genre[]>([]);
    const [genreFilterInput, setGenreFilterInput] = useState("");
    const [genreFilterDropdownOpen, setGenreFilterDropdownOpen] = useState(false);
    const [allGenres, setAllGenres] = useState<Genre[]>([]);
    const [ratingMin, setRatingMin] = useState("");
    const [ratingMax, setRatingMax] = useState("");

    useEffect(() => {
        genreClient.getAllGenres().then(setAllGenres).catch(() => {});
    }, []);

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

                const withGenres = await Promise.all(
                    moviesData.map(async m => ({
                        ...m,
                        genres: await movieClient.getMovieGenres(m.id).catch(() => []),
                    }))
                );

                setMovies(withGenres as MovieWithSeen[]);
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

    const applyFiltersAndSort = (list: MovieWithSeen[]) => {
        let result = list;
        if (search.trim()) {
            const q = search.trim().toLowerCase();
            result = result.filter(m => m.title?.toLowerCase().includes(q));
        }
        if (sortBy === "genre" && selectedFilterGenres.length > 0) {
            result = result.filter(m =>
                selectedFilterGenres.every(fg => m.genres?.some(g => g.id === fg.id))
            );
        }
        if (ratingMin !== "" || ratingMax !== "") {
            const min = ratingMin !== "" ? parseInt(ratingMin) : 0;
            const max = ratingMax !== "" ? parseInt(ratingMax) : 10;
            result = result.filter(m => { const r = m.rating ?? 0; return r >= min && r <= max; });
        }
        if (sortBy === "name-asc") result = [...result].sort((a, b) => (a.title ?? "").localeCompare(b.title ?? ""));
        if (sortBy === "name-desc") result = [...result].sort((a, b) => (b.title ?? "").localeCompare(a.title ?? ""));
        if (sortBy === "rating-asc") result = [...result].sort((a, b) => (a.rating ?? 0) - (b.rating ?? 0));
        if (sortBy === "rating-desc") result = [...result].sort((a, b) => (b.rating ?? 0) - (a.rating ?? 0));
        return result;
    };

    const seenMovies = applyFiltersAndSort(movies.filter(m => m.seen === true));
    const watchlistMovies = applyFiltersAndSort(movies.filter(m => m.seen !== true));

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

                {/* Search + Sort toolbar */}
                <div style={{ display: "flex", gap: "10px", marginTop: "12px", alignItems: "flex-start", flexWrap: "wrap" }}>
                    <input
                        placeholder="Search movies..."
                        value={search}
                        onChange={e => setSearch(e.target.value)}
                        style={{
                            flex: 1, minWidth: "180px",
                            padding: "7px 12px", borderRadius: "6px",
                            border: "1px solid #333", background: "#111",
                            color: "#fff", fontSize: "0.9rem",
                        }}
                    />
                    <div style={{ position: "relative" }}>
                        <button
                            onClick={() => setSortOpen(o => !o)}
                            style={{
                                padding: "7px 14px", borderRadius: "6px",
                                background: sortBy ? "#e50914" : "#2a2a2a",
                                color: "#fff", border: "none", cursor: "pointer",
                                fontSize: "0.9rem", whiteSpace: "nowrap",
                            }}
                        >
                            Sort {sortBy ? "▲" : "▼"}
                        </button>
                        {sortOpen && (
                            <div style={{
                                position: "absolute", top: "100%", right: 0,
                                background: "#2a2a2a", borderRadius: "8px",
                                border: "1px solid #444", zIndex: 20,
                                marginTop: "4px", minWidth: "200px", overflow: "hidden",
                            }}>
                                {(["name-asc", "name-desc", "rating-desc", "rating-asc", "genre"] as const).map(opt => {
                                    const labels: Record<string, string> = {
                                        "name-asc": "Name (A → Z)", "name-desc": "Name (Z → A)",
                                        "rating-desc": "Rating (high → low)", "rating-asc": "Rating (low → high)",
                                        "genre": "Filter by genre",
                                    };
                                    return (
                                        <div
                                            key={opt}
                                            onMouseDown={() => { setSortBy(sortBy === opt ? "" : opt); if (opt !== "genre") setSortOpen(false); }}
                                            style={{
                                                padding: "9px 14px", cursor: "pointer", fontSize: "0.88rem",
                                                background: sortBy === opt ? "#444" : "transparent",
                                                fontWeight: sortBy === opt ? "bold" : "normal",
                                            }}
                                            onMouseEnter={e => (e.currentTarget.style.background = sortBy === opt ? "#444" : "#333")}
                                            onMouseLeave={e => (e.currentTarget.style.background = sortBy === opt ? "#444" : "transparent")}
                                        >
                                            {labels[opt]}
                                        </div>
                                    );
                                })}
                                <div style={{ padding: "8px 14px", borderTop: "1px solid #444" }}>
                                    <div style={{ fontSize: "0.78rem", color: "#aaa", marginBottom: "6px" }}>Rating range</div>
                                    <div style={{ display: "flex", gap: "6px", alignItems: "center" }}>
                                        <input type="number" min="0" max="10" placeholder="Min" value={ratingMin} onChange={e => setRatingMin(e.target.value)}
                                            style={{ width: "56px", padding: "4px 6px", borderRadius: "4px", border: "1px solid #555", background: "#111", color: "#fff", fontSize: "0.85rem" }} />
                                        <span style={{ color: "#aaa" }}>–</span>
                                        <input type="number" min="0" max="10" placeholder="Max" value={ratingMax} onChange={e => setRatingMax(e.target.value)}
                                            style={{ width: "56px", padding: "4px 6px", borderRadius: "4px", border: "1px solid #555", background: "#111", color: "#fff", fontSize: "0.85rem" }} />
                                    </div>
                                </div>
                                {sortBy && (
                                    <div
                                        onMouseDown={() => { setSortBy(""); setSelectedFilterGenres([]); setGenreFilterInput(""); setRatingMin(""); setRatingMax(""); setSortOpen(false); }}
                                        style={{ padding: "8px 14px", cursor: "pointer", fontSize: "0.82rem", color: "#e50914", borderTop: "1px solid #444" }}
                                        onMouseEnter={e => (e.currentTarget.style.background = "#333")}
                                        onMouseLeave={e => (e.currentTarget.style.background = "transparent")}
                                    >
                                        Clear sorting
                                    </div>
                                )}
                            </div>
                        )}
                    </div>
                </div>

                {sortBy === "genre" && (
                    <div style={{ marginTop: "8px", position: "relative" }}>
                        <input
                            autoFocus
                            placeholder="Search genres..."
                            value={genreFilterInput}
                            onChange={e => { setGenreFilterInput(e.target.value); setGenreFilterDropdownOpen(e.target.value.length > 0); }}
                            onFocus={() => { if (genreFilterInput.length > 0) setGenreFilterDropdownOpen(true); }}
                            onBlur={() => setTimeout(() => setGenreFilterDropdownOpen(false), 150)}
                            style={{
                                width: "100%", boxSizing: "border-box",
                                padding: "7px 12px", borderRadius: "6px",
                                border: "1px solid #e50914", background: "#111",
                                color: "#fff", fontSize: "0.9rem",
                            }}
                            autoComplete="off"
                        />
                        {genreFilterDropdownOpen && (() => {
                            const filtered = allGenres
                                .filter(g => g.name?.toLowerCase().includes(genreFilterInput.toLowerCase()) && !selectedFilterGenres.some(s => s.id === g.id))
                                .slice(0, 5);
                            return filtered.length > 0 ? (
                                <div style={{
                                    position: "absolute", top: "100%", left: 0, right: 0,
                                    background: "#2a2a2a", borderRadius: "6px",
                                    border: "1px solid #444", zIndex: 30,
                                    overflow: "hidden", marginTop: "2px",
                                }}>
                                    {filtered.map(g => (
                                        <div key={g.id}
                                            onMouseDown={() => { setSelectedFilterGenres(prev => [...prev, g]); setGenreFilterInput(""); setGenreFilterDropdownOpen(false); }}
                                            style={{ padding: "8px 12px", cursor: "pointer", fontSize: "0.88rem" }}
                                            onMouseEnter={e => (e.currentTarget.style.background = "#3a3a3a")}
                                            onMouseLeave={e => (e.currentTarget.style.background = "transparent")}
                                        >{g.name}</div>
                                    ))}
                                </div>
                            ) : null;
                        })()}
                        {selectedFilterGenres.length > 0 && (
                            <div style={{ display: "flex", flexWrap: "wrap", gap: "6px", marginTop: "8px" }}>
                                {selectedFilterGenres.map(g => (
                                    <span key={g.id} style={{ background: "#333", borderRadius: "20px", padding: "3px 10px", fontSize: "0.8rem", display: "flex", alignItems: "center", gap: "6px" }}>
                                        {g.name}
                                        <button type="button" onClick={() => setSelectedFilterGenres(prev => prev.filter(s => s.id !== g.id))}
                                            style={{ background: "none", border: "none", color: "#aaa", cursor: "pointer", padding: 0, fontSize: "0.9rem", lineHeight: 1 }}>×</button>
                                    </span>
                                ))}
                            </div>
                        )}
                    </div>
                )}
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
                    {movie.genres && movie.genres.length > 0 && (
                        <div style={{ display: "flex", flexWrap: "wrap", gap: "4px", alignItems: "center" }}>
                            {movie.genres.slice(0, 3).map(g => (
                                <span key={g.id} style={{
                                    background: "#2a2a2a", borderRadius: "4px",
                                    padding: "1px 6px", fontSize: "0.7rem", color: "#aaa",
                                }}>{g.name}</span>
                            ))}
                            {movie.genres.length > 3 && (
                                <span style={{ fontSize: "0.7rem", color: "#666" }}>and {movie.genres.length - 3} more</span>
                            )}
                        </div>
                    )}
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
