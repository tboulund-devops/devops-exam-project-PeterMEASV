import { useEffect, useRef, useState } from "react";
import { useAtomValue } from "jotai";
import { useNavigate, useLocation } from "react-router";
import { finalUrl, genreClient, movieClient } from "./baseUrl.ts";
import type { Genre, Movie } from "./generated-ts-client.ts";
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
type MovieWithSeen = Movie & { seen?: boolean | null; rating?: number | null; genres?: Genre[] };

// ... existing code for CreateMovieModal ...

type CreateMovieModalProps = {
    onClose: () => void;
    onCreated: (movie: Movie, rating?: number) => void;
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

    const [genreInput, setGenreInput] = useState("");
    const [selectedGenres, setSelectedGenres] = useState<Genre[]>([]);
    const [allGenres, setAllGenres] = useState<Genre[]>([]);
    const [dropdownOpen, setDropdownOpen] = useState(false);
    const dragIndex = useRef<number | null>(null);

    useEffect(() => {
        genreClient.getAllGenres().then(setAllGenres).catch(() => {});
    }, []);

    const filteredGenres = allGenres
        .filter(g =>
            g.name?.toLowerCase().includes(genreInput.toLowerCase()) &&
            !selectedGenres.some(s => s.id === g.id)
        )
        .slice(0, 5);

    const addGenre = (genre: Genre) => {
        if (!selectedGenres.some(g => g.id === genre.id)) {
            setSelectedGenres(prev => [...prev, genre]);
        }
        setGenreInput("");
        setDropdownOpen(false);
    };

    const handleGenreKeyDown = async (e: React.KeyboardEvent<HTMLInputElement>) => {
        if (e.key !== "Enter") return;
        e.preventDefault();
        const name = genreInput.trim();
        if (!name) return;

        const existing = allGenres.find(g => g.name?.toLowerCase() === name.toLowerCase());
        if (existing) {
            addGenre(existing);
            return;
        }

        try {
            const created = await genreClient.createGenre(name);
            setAllGenres(prev => [...prev, created]);
            addGenre(created);
        } catch {
            setError("Could not create genre.");
        }
    };

    const removeGenre = (id: string | undefined) => {
        setSelectedGenres(prev => prev.filter(g => g.id !== id));
    };

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
            if (rating) url += `&rating=${encodeURIComponent(rating)}`;
            selectedGenres.forEach((g, i) => {
                if (g.id) url += `&genres[${i}].id=${encodeURIComponent(g.id)}`;
                if (g.name) url += `&genres[${i}].name=${encodeURIComponent(g.name)}`;
            });

            const token = tokenStorage.getItem(TOKEN_KEY, null);
            const response = await fetch(url, {
                method: "POST",
                headers: token ? { Authorization: `Bearer ${token}` } : {},
                body: formData,
            });
            if (!response.ok) throw new Error("Failed");
            const created: Movie = await response.json();
            onCreated(created, rating ? parseInt(rating) : undefined);
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

                    {/* Genre input */}
                    <div style={{ position: "relative" }}>
                        <input
                            placeholder="Genres (type and press Enter)"
                            value={genreInput}
                            onChange={e => { setGenreInput(e.target.value); setDropdownOpen(true); }}
                            onKeyDown={handleGenreKeyDown}
                            onFocus={() => setDropdownOpen(true)}
                            onBlur={() => setTimeout(() => setDropdownOpen(false), 150)}
                            style={inputStyle}
                            autoComplete="off"
                        />
                        {dropdownOpen && filteredGenres.length > 0 && (
                            <div style={{
                                position: "absolute", top: "100%", left: 0, right: 0,
                                background: "#2a2a2a", borderRadius: "6px",
                                border: "1px solid #444", zIndex: 10,
                                overflow: "hidden", marginTop: "2px",
                            }}>
                                {filteredGenres.map(g => (
                                    <div
                                        key={g.id}
                                        onMouseDown={() => addGenre(g)}
                                        style={{
                                            padding: "8px 12px", cursor: "pointer",
                                            fontSize: "0.9rem",
                                        }}
                                        onMouseEnter={e => (e.currentTarget.style.background = "#3a3a3a")}
                                        onMouseLeave={e => (e.currentTarget.style.background = "transparent")}
                                    >
                                        {g.name}
                                    </div>
                                ))}
                            </div>
                        )}
                    </div>
                    {selectedGenres.length > 0 && (
                        <div style={{ display: "flex", flexWrap: "wrap", gap: "6px" }}>
                            {selectedGenres.map((g, i) => (
                                <span
                                    key={g.id}
                                    draggable
                                    onDragStart={() => { dragIndex.current = i; }}
                                    onDragOver={e => e.preventDefault()}
                                    onDrop={() => {
                                        if (dragIndex.current === null || dragIndex.current === i) return;
                                        const reordered = [...selectedGenres];
                                        const [moved] = reordered.splice(dragIndex.current, 1);
                                        reordered.splice(i, 0, moved);
                                        setSelectedGenres(reordered);
                                        dragIndex.current = null;
                                    }}
                                    style={{
                                        background: "#333", borderRadius: "20px",
                                        padding: "3px 10px", fontSize: "0.8rem",
                                        display: "flex", alignItems: "center", gap: "6px",
                                        cursor: "grab",
                                    }}
                                >
                                    {g.name}
                                    <button
                                        type="button"
                                        onClick={() => removeGenre(g.id)}
                                        style={{
                                            background: "none", border: "none",
                                            color: "#aaa", cursor: "pointer",
                                            padding: 0, fontSize: "0.9rem", lineHeight: 1,
                                        }}
                                    >×</button>
                                </span>
                            ))}
                        </div>
                    )}

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
                        <span style={{ fontSize: "0.7rem", color: "#666" }}>
                            and {movie.genres.length - 3} more
                        </span>
                    )}
                </div>
            )}
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
            const withGenres = await Promise.all(
                moviesData.map(async m => ({
                    ...m,
                    genres: await movieClient.getMovieGenres(m.id).catch(() => []),
                }))
            );
            setMovies(withGenres as MovieWithSeen[]);
        } catch {
            setError("Could not load movies.");
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        fetchMovies();
    }, [userId]);

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

    const handleCreated = async (movie: Movie, rating?: number) => {
        const genres = await movieClient.getMovieGenres(movie.id).catch(() => []);
        setMovies(prev => [...prev, { ...movie, seen: false, genres, rating: rating ?? null }]);
        setModalOpen(false);
    };

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
            result = result.filter(m => {
                const r = m.rating ?? 0;
                return r >= min && r <= max;
            });
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
                                marginTop: "4px", minWidth: "200px",
                                overflow: "hidden",
                            }}>
                                {(["name-asc", "name-desc", "rating-desc", "rating-asc", "genre"] as const).map(opt => {
                                    const labels: Record<string, string> = {
                                        "name-asc": "Name (A → Z)",
                                        "name-desc": "Name (Z → A)",
                                        "rating-desc": "Rating (high → low)",
                                        "rating-asc": "Rating (low → high)",
                                        "genre": "Filter by genre",
                                    };
                                    return (
                                        <div
                                            key={opt}
                                            onMouseDown={() => { setSortBy(sortBy === opt ? "" : opt); if (opt !== "genre") setSortOpen(false); }}
                                            style={{
                                                padding: "9px 14px", cursor: "pointer",
                                                fontSize: "0.88rem",
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

                                {/* Rating range inputs */}
                                <div style={{ padding: "8px 14px", borderTop: "1px solid #444" }}>
                                    <div style={{ fontSize: "0.78rem", color: "#aaa", marginBottom: "6px" }}>Rating range</div>
                                    <div style={{ display: "flex", gap: "6px", alignItems: "center" }}>
                                        <input
                                            type="number" min="0" max="10" placeholder="Min"
                                            value={ratingMin}
                                            onChange={e => setRatingMin(e.target.value)}
                                            style={{ width: "56px", padding: "4px 6px", borderRadius: "4px", border: "1px solid #555", background: "#111", color: "#fff", fontSize: "0.85rem" }}
                                        />
                                        <span style={{ color: "#aaa" }}>–</span>
                                        <input
                                            type="number" min="0" max="10" placeholder="Max"
                                            value={ratingMax}
                                            onChange={e => setRatingMax(e.target.value)}
                                            style={{ width: "56px", padding: "4px 6px", borderRadius: "4px", border: "1px solid #555", background: "#111", color: "#fff", fontSize: "0.85rem" }}
                                        />
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

                {/* Genre filter (shown when genre sort is active) */}
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
                                .filter(g =>
                                    g.name?.toLowerCase().includes(genreFilterInput.toLowerCase()) &&
                                    !selectedFilterGenres.some(s => s.id === g.id)
                                )
                                .slice(0, 5);
                            return filtered.length > 0 ? (
                                <div style={{
                                    position: "absolute", top: "100%", left: 0, right: 0,
                                    background: "#2a2a2a", borderRadius: "6px",
                                    border: "1px solid #444", zIndex: 30,
                                    overflow: "hidden", marginTop: "2px",
                                }}>
                                    {filtered.map(g => (
                                        <div
                                            key={g.id}
                                            onMouseDown={() => {
                                                setSelectedFilterGenres(prev => [...prev, g]);
                                                setGenreFilterInput("");
                                                setGenreFilterDropdownOpen(false);
                                            }}
                                            style={{ padding: "8px 12px", cursor: "pointer", fontSize: "0.88rem" }}
                                            onMouseEnter={e => (e.currentTarget.style.background = "#3a3a3a")}
                                            onMouseLeave={e => (e.currentTarget.style.background = "transparent")}
                                        >
                                            {g.name}
                                        </div>
                                    ))}
                                </div>
                            ) : null;
                        })()}
                        {selectedFilterGenres.length > 0 && (
                            <div style={{ display: "flex", flexWrap: "wrap", gap: "6px", marginTop: "8px" }}>
                                {selectedFilterGenres.map(g => (
                                    <span key={g.id} style={{
                                        background: "#333", borderRadius: "20px",
                                        padding: "3px 10px", fontSize: "0.8rem",
                                        display: "flex", alignItems: "center", gap: "6px",
                                    }}>
                                        {g.name}
                                        <button
                                            type="button"
                                            onClick={() => setSelectedFilterGenres(prev => prev.filter(s => s.id !== g.id))}
                                            style={{ background: "none", border: "none", color: "#aaa", cursor: "pointer", padding: 0, fontSize: "0.9rem", lineHeight: 1 }}
                                        >×</button>
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
