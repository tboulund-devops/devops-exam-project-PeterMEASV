import { useEffect, useRef, useState } from "react";
import { useAtomValue } from "jotai";
import { useNavigate, useLocation } from "react-router";
import { finalUrl, movieClient } from "./baseUrl.ts";
import type { Movie } from "./generated-ts-client.ts";
import { TOKEN_KEY, tokenAtom, tokenStorage, userInfoAtom } from "./Token.tsx";

function getUserIdFromToken(token: string | null): string | null {
    if (!token) return null;
    try {
        const payload = JSON.parse(atob(token.split('.')[1].replace(/-/g, '+').replace(/_/g, '/')));
        return payload.sub ?? null;
    } catch {
        return null;
    }
}

// Extended Movie type with seen status
type MovieWithSeen = Movie & { seen?: boolean | null; rating?: number | null };

// ... existing code for CreateMovieModal ...

type CreateMovieModalProps = {
    onClose: () => void;
    onCreated: (movie: Movie) => void;
};

function CreateMovieModal({ onClose, onCreated }: CreateMovieModalProps) {
    const userInfo = useAtomValue(userInfoAtom);
    const [title, setTitle] = useState("");
    const [year, setYear] = useState("");
    const [description, setDescription] = useState("");
    const [starring, setStarring] = useState("");
    const [rating, setRating] = useState("");
    const [photoFile, setPhotoFile] = useState<File | null>(null);
    const [preview, setPreview] = useState<string | null>(null);
    const [submitting, setSubmitting] = useState(false);
    const [error, setError] = useState<string | null>(null);
    const inputRef = useRef<HTMLInputElement>(null);

    const handlePhotoChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        const file = e.target.files?.[0] ?? null;
        setPhotoFile(file);
        setPreview(file ? URL.createObjectURL(file) : null);
    };

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        if (!title.trim() || !year.trim()) return;

        if (rating && (parseInt(rating) < 1 || parseInt(rating) > 10)) {
            setError("Rating must be between 1 and 10");
            return;
        }

        setSubmitting(true);
        setError(null);

        try {
            let userId = userInfo?.id;
            if (!userId) {
                userId = getUserIdFromToken(tokenStorage.getItem(TOKEN_KEY, null)) ?? undefined;
            }
            if (!userId) {
                setError("Session expired. Please log in again.");
                return;
            }

            const formData = new FormData();
            formData.append("title", title.trim());
            formData.append("year", year);
            if (description.trim()) formData.append("description", description.trim());
            if (starring.trim()) formData.append("starring", starring.trim());
            if (photoFile) formData.append("photo", photoFile, photoFile.name);

            let url = `${finalUrl}/Movie/CreateMovie?userID=${encodeURIComponent(userId)}`;
            if (rating) {
                url += `&rating=${encodeURIComponent(rating)}`;
            }

            const token = tokenStorage.getItem(TOKEN_KEY, null);
            const response = await fetch(url, {
                method: "POST",
                headers: token ? { Authorization: `Bearer ${token}` } : {},
                body: formData,
            });
            if (!response.ok) throw new Error("Failed");
            const created: Movie = await response.json();
            onCreated(created);
        } catch {
            setError("Could not create movie. Please try again.");
        } finally {
            setSubmitting(false);
        }
    };

    return (
        <div style={{
            position: "fixed", inset: 0,
            background: "rgba(0,0,0,0.6)",
            display: "flex", alignItems: "center", justifyContent: "center",
            zIndex: 100,
        }} onClick={onClose}>
            <div style={{
                background: "#1a1a1a", borderRadius: "10px",
                padding: "28px", width: "100%", maxWidth: "440px",
                display: "flex", flexDirection: "column", gap: "14px",
            }} onClick={(e) => e.stopPropagation()}>

                <h2 style={{ margin: 0 }}>Add movie</h2>

                <form onSubmit={handleSubmit} style={{ display: "flex", flexDirection: "column", gap: "12px" }}>
                    <input
                        placeholder="Title *"
                        value={title}
                        onChange={e => setTitle(e.target.value)}
                        required
                        style={inputStyle}
                    />
                    <input
                        placeholder="Year *"
                        type="number"
                        value={year}
                        onChange={e => setYear(e.target.value)}
                        required
                        style={inputStyle}
                    />
                    <input
                        placeholder="Starring"
                        value={starring}
                        onChange={e => setStarring(e.target.value)}
                        style={inputStyle}
                    />
                    <textarea
                        placeholder="Description"
                        value={description}
                        onChange={e => setDescription(e.target.value)}
                        rows={3}
                        style={{ ...inputStyle, resize: "vertical" }}
                    />
                    <input
                        placeholder="Rating (1-10, optional)"
                        type="number"
                        min="1"
                        max="10"
                        value={rating}
                        onChange={e => setRating(e.target.value)}
                        style={inputStyle}
                    />

                    <div style={{ display: "flex", flexDirection: "column", gap: "8px" }}>
                        <label htmlFor="poster-upload" style={{ fontSize: "0.85rem", color: "#aaa" }}>Poster (optional)</label>
                        <input id="poster-upload" ref={inputRef} type="file" accept="image/*" onChange={handlePhotoChange} />
                        {preview && (
                            <img src={preview} alt="Preview" style={{ maxHeight: 160, objectFit: "contain", borderRadius: "6px" }} />
                        )}
                    </div>

                    {error && <p style={{ color: "red", margin: 0 }}>{error}</p>}

                    <div style={{ display: "flex", gap: "10px", justifyContent: "flex-end" }}>
                        <button type="button" onClick={onClose} style={secondaryButtonStyle}>Cancel</button>
                        <button type="submit" disabled={submitting} style={primaryButtonStyle}>
                            {submitting ? "Saving..." : "Create"}
                        </button>
                    </div>
                </form>
            </div>
        </div>
    );
}

// ── Movie Card Component ──────────────────────────────────────────────────────

type MovieCardProps = {
    movie: MovieWithSeen;
    onClick: () => void;
    showSeenBadge?: boolean;
};

function MovieCard({ movie, onClick, showSeenBadge }: MovieCardProps) {
    return (
        <div
            onClick={onClick}
            style={{
                position: "relative",
                display: "flex",
                flexDirection: "column",
                gap: "8px",
                cursor: "pointer",
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
            <span style={{ fontWeight: "bold", fontSize: "0.9rem" }}>{movie.title}</span>
            <span style={{ fontSize: "0.8rem", color: "#aaa" }}>{movie.year}</span>
        </div>
    );
}

// ── Dashboard ─────────────────────────────────────────────────────────────────

type TabType = "watchlist" | "seen";

function Dashboard() {
    const token = useAtomValue(tokenAtom);
    const userId = useAtomValue(userInfoAtom)?.id ?? getUserIdFromToken(token);
    const [movies, setMovies] = useState<MovieWithSeen[]>([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);
    const [modalOpen, setModalOpen] = useState(false);
    const location = useLocation();
    const navigate = useNavigate();
    
    // Get tab from location state or default to watchlist
    const [activeTab, setActiveTab] = useState<TabType>(
        (location.state?.returnTab as TabType) || "watchlist"
    );

    const fetchMovies = async () => {
        if (!userId) {
            setLoading(false);
            return;
        }
        
        setLoading(true);
        try {
            const moviesData = await movieClient.getMoviesByUser(userId);
            setMovies(moviesData as MovieWithSeen[]);
        } catch {
            setError("Could not load movies.");
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        fetchMovies();
    }, [userId]);

    const handleCreated = (movie: Movie) => {
        setMovies(prev => [...prev, { ...movie, seen: false }]);
        setModalOpen(false);
    };

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
                <div style={{
                    display: "flex",
                    gap: "24px",
                    alignItems: "center",
                }}>
                    <h1 style={{ margin: 0, fontSize: "1.5rem" }}>My Collection</h1>
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
                                        <h2 style={{ margin: "0 0 8px 0", color: "#aaa" }}>Your watchlist is empty</h2>
                                        <p>Add movies you want to watch</p>
                                    </div>
                                ) : (
                                    <div style={{
                                        display: "grid",
                                        gridTemplateColumns: "repeat(auto-fill, minmax(180px, 1fr))",
                                        gap: "20px",
                                    }}>
                                        {watchlistMovies.map(movie => (
                                            <MovieCard
                                                key={movie.id}
                                                movie={movie}
                                                onClick={() => navigate(`/movie/${movie.id}`, { 
                                                    state: { movie, returnTab: "watchlist" } 
                                                })}
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
                                        <p>Movies you mark as watched will appear here</p>
                                    </div>
                                ) : (
                                    <div style={{
                                        display: "grid",
                                        gridTemplateColumns: "repeat(auto-fill, minmax(180px, 1fr))",
                                        gap: "20px",
                                    }}>
                                        {seenMovies.map(movie => (
                                            <MovieCard
                                                key={movie.id}
                                                movie={movie}
                                                onClick={() => navigate(`/movie/${movie.id}`, { 
                                                    state: { movie, returnTab: "seen" } 
                                                })}
                                                showSeenBadge
                                            />
                                        ))}
                                    </div>
                                )}
                            </>
                        )}
                    </>
                )}
            </div>

            {/* Floating Add Button */}
            <button
                onClick={() => setModalOpen(true)}
                style={{
                    position: "fixed", bottom: "28px", right: "28px",
                    width: "56px", height: "56px", borderRadius: "50%",
                    fontSize: "28px", lineHeight: 1,
                    background: "#e50914", color: "#fff",
                    border: "none", cursor: "pointer",
                    boxShadow: "0 4px 12px rgba(0,0,0,0.4)",
                    display: "flex", alignItems: "center", justifyContent: "center",
                    transition: "transform 0.2s ease",
                }}
                onMouseEnter={(e) => e.currentTarget.style.transform = "scale(1.1)"}
                onMouseLeave={(e) => e.currentTarget.style.transform = "scale(1)"}
                aria-label="Add movie"
            >
                +
            </button>

            {modalOpen && (
                <CreateMovieModal
                    onClose={() => setModalOpen(false)}
                    onCreated={handleCreated}
                />
            )}
        </div>
    );
}

// ── Styles ────────────────────────────────────────────────────────────────────

const inputStyle: React.CSSProperties = {
    padding: "8px 12px",
    borderRadius: "6px",
    border: "1px solid #333",
    background: "#111",
    color: "#fff",
    fontSize: "0.95rem",
    width: "100%",
    boxSizing: "border-box",
};

const primaryButtonStyle: React.CSSProperties = {
    padding: "8px 20px",
    borderRadius: "6px",
    background: "#e50914",
    color: "#fff",
    border: "none",
    cursor: "pointer",
    fontWeight: "bold",
};

const secondaryButtonStyle: React.CSSProperties = {
    padding: "8px 20px",
    borderRadius: "6px",
    background: "#333",
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

export default Dashboard;
