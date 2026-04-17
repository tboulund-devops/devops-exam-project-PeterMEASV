import { useEffect, useRef, useState } from "react";
import { useLocation, useNavigate } from "react-router";
import { useAtomValue } from "jotai";
import { genreClient, movieClient } from "./baseUrl.ts";
import type { Genre, Movie } from "./generated-ts-client.ts";
import { tokenAtom, userInfoAtom } from "./Token.tsx";

function getUserIdFromToken(token: string | null): string | null {
    if (!token) return null;
    try {
        const payload = JSON.parse(atob(token.split('.')[1].replace(/-/g, '+').replace(/_/g, '/')));
        return payload.sub ?? null;
    } catch { return null; }
}

type MovieWithSeen = Movie & { seen?: boolean | null; rating?: number | null };

function MovieDetails() {
    const location = useLocation();
    const navigate = useNavigate();
    const token = useAtomValue(tokenAtom);
    const userId = useAtomValue(userInfoAtom)?.id ?? getUserIdFromToken(token);

    const [movie, setMovie] = useState<MovieWithSeen | null>(location.state?.movie ?? null);
    const [title, setTitle] = useState("");
    const [year, setYear] = useState("");
    const [description, setDescription] = useState("");
    const [starring, setStarring] = useState("");
    const [rating, setRating] = useState("");
    const [photoPreview, setPhotoPreview] = useState<string | null>(null);
    const [saving, setSaving] = useState(false);
    const [saved, setSaved] = useState(false);
    const [error, setError] = useState<string | null>(null);
    const inputRef = useRef<HTMLInputElement>(null);
    const [movieRating, setMovieRating] = useState<number | null>(null);
    const [isSeen, setIsSeen] = useState<boolean>(false);
    const [updatingSeen, setUpdatingSeen] = useState(false);

    const [selectedGenres, setSelectedGenres] = useState<Genre[]>([]);
    const [allGenres, setAllGenres] = useState<Genre[]>([]);
    const [genreInput, setGenreInput] = useState("");
    const [genreDropdownOpen, setGenreDropdownOpen] = useState(false);
    
    // Store the return tab from navigation state
    const returnTab = location.state?.returnTab || "watchlist";

    useEffect(() => {
        if (!movie) return;
        setTitle(movie.title ?? "");
        setYear(String(movie.year ?? ""));
        setDescription(movie.description ?? "");
        setStarring(movie.starring ?? "");
        setPhotoPreview(movie.photo ?? null);
        setRating("");
        setIsSeen(movie.seen ?? false);
        
        movieClient.getMovieRatingByUser(userId ?? undefined, movie.id).then(rating => {
            setMovieRating(rating);
            setRating(String(rating ?? ""));
        }).catch(() => {
            setMovieRating(null);
        });

        movieClient.getMovieGenres(movie.id).then(setSelectedGenres).catch(() => {});
        genreClient.getAllGenres().then(setAllGenres).catch(() => {});
    }, [movie?.id, userId]);

    if (!movie) {
        return (
            <div style={{ padding: "32px", color: "#fff" }}>
                <p>Movie not found.</p>
                <button 
                    onClick={() => navigate("/dashboard", { state: { returnTab } })} 
                    style={secondaryButtonStyle}
                >
                    ← Back
                </button>
            </div>
        );
    }

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
        setGenreDropdownOpen(false);
    };

    const handleGenreKeyDown = async (e: React.KeyboardEvent<HTMLInputElement>) => {
        if (e.key !== "Enter") return;
        e.preventDefault();
        const name = genreInput.trim();
        if (!name) return;

        const existing = allGenres.find(g => g.name?.toLowerCase() === name.toLowerCase());
        if (existing) { addGenre(existing); return; }

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
        const file = e.target.files?.[0];
        if (file) setPhotoPreview(URL.createObjectURL(file));
    };

    const handleSave = async (e: React.FormEvent) => {
        e.preventDefault();
        setSaving(true);
        setSaved(false);
        setError(null);
        try {
            if (rating && userId && movie.id) {
                await movieClient.updateMovieRating(userId ?? undefined, movie.id, parseInt(rating));
                setMovieRating(parseInt(rating));
            }

            await movieClient.updateMovieGenres(movie.id, selectedGenres);

            const updated = await movieClient.editMovie({
                ...movie,
                title: title.trim(),
                year: year ? parseInt(year) as any : movie.year,
                description: description.trim() || undefined,
                starring: starring.trim() || undefined,
            });
            setMovie({ ...updated, seen: isSeen, rating: movieRating });

            setSaved(true);
            setTimeout(() => setSaved(false), 2500);
        } catch {
            setError("Could not save changes. Please try again.");
        } finally {
            setSaving(false);
        }
    };

    const handleSeenToggle = async () => {
        if (!userId || !movie?.id) {
            setError("You must be logged in to update seen status.");
            return;
        }
        setUpdatingSeen(true);
        setError(null);
        try {
            const newSeenStatus = !isSeen;
            await movieClient.addMovieToSeen(movie.id, userId ?? undefined, newSeenStatus);
            setIsSeen(newSeenStatus);
            setMovie({ ...movie, seen: newSeenStatus });
            
            // Show success message
            setSaved(true);
            setTimeout(() => setSaved(false), 2000);
        } catch {
            setError("Could not update seen status. Please try again.");
        } finally {
            setUpdatingSeen(false);
        }
    };

    const removeMovieFromUser = async () => {
        if (!userId || !movie?.id) {
            setError("You must be logged in to remove movies from your collection.");
            return;
        }
        const confirmed = window.confirm(`Remove "${title}" from your collection?`);
        if (!confirmed) return;
        try {
            await movieClient.removeMovieFromUser(userId ?? undefined, movie.id);
            navigate("/dashboard", { state: { returnTab } });
        } catch {
            setError("Could not remove movie. Please try again.");
        }
    };

    const handleBackClick = () => {
        navigate("/dashboard", { state: { returnTab } });
    };

    return (
        <div style={{ minHeight: "100vh", background: "#111", color: "#fff", padding: "32px" }}>
            <button onClick={handleBackClick} style={secondaryButtonStyle}>← Back</button>

            <div style={{
                display: "flex", gap: "40px", marginTop: "28px",
                flexWrap: "wrap",
            }}>
                {/* Poster */}
                <div style={{ display: "flex", flexDirection: "column", gap: "12px", flexShrink: 0 }}>
                    <div style={{ position: "relative" }}>
                        {photoPreview
                            ? <img src={photoPreview} alt={title} style={{ width: "220px", aspectRatio: "2/3", objectFit: "cover", borderRadius: "8px" }} />
                            : <div style={{ width: "220px", aspectRatio: "2/3", background: "#2a2a2a", borderRadius: "8px", display: "flex", alignItems: "center", justifyContent: "center", color: "#666" }}>No poster</div>
                        }
                        {movieRating && movieRating > 0 && (
                            <div style={{
                                position: "absolute",
                                top: "12px",
                                left: "12px",
                                background: "rgba(255, 193, 7, 0.95)",
                                color: "#000",
                                borderRadius: "8px",
                                padding: "8px 12px",
                                display: "flex",
                                alignItems: "center",
                                justifyContent: "center",
                                fontWeight: "bold",
                                fontSize: "18px",
                                boxShadow: "0 2px 8px rgba(0,0,0,0.4)",
                                gap: "6px",
                            }}>
                                <span>⭐</span>
                                <span>{rating}</span>
                            </div>
                        )}
                        {isSeen && (
                            <div style={{
                                position: "absolute",
                                top: "12px",
                                right: "12px",
                                background: "#4caf50",
                                color: "#fff",
                                borderRadius: "50%",
                                width: "36px",
                                height: "36px",
                                display: "flex",
                                alignItems: "center",
                                justifyContent: "center",
                                fontWeight: "bold",
                                fontSize: "20px",
                                boxShadow: "0 2px 8px rgba(0,0,0,0.4)",
                            }}>
                                ✓
                            </div>
                        )}
                    </div>
                    <button onClick={() => inputRef.current?.click()} style={secondaryButtonStyle}>Change poster</button>
                    <input ref={inputRef} type="file" accept="image/*" style={{ display: "none" }} onChange={handlePhotoChange} />

                    {/* Seen Toggle Button */}
                    <button 
                        onClick={handleSeenToggle} 
                        disabled={updatingSeen}
                        style={{
                            ...secondaryButtonStyle,
                            background: isSeen ? "#4caf50" : "#2a2a2a",
                            color: "#fff",
                            display: "flex",
                            alignItems: "center",
                            justifyContent: "center",
                            gap: "8px",
                            fontWeight: isSeen ? "bold" : "normal",
                        }}
                    >
                        {updatingSeen ? "Updating..." : (
                            <>
                                <span>{isSeen ? "✓" : "○"}</span>
                                <span>{isSeen ? "Watched" : "Mark as watched"}</span>
                            </>
                        )}
                    </button>

                    <button onClick={removeMovieFromUser} style={{ ...secondaryButtonStyle, color: "#e50914" }}>Remove from collection</button>
                </div>

                {/* Form */}
                <form onSubmit={handleSave} style={{ flex: 1, minWidth: "280px", display: "flex", flexDirection: "column", gap: "16px" }}>
                    <div style={{ display: "flex", flexDirection: "column", gap: "4px" }}>
                        <label style={labelStyle}>Title</label>
                        <input value={title} onChange={e => setTitle(e.target.value)} required style={inputStyle} />
                    </div>

                    <div style={{ display: "flex", flexDirection: "column", gap: "4px" }}>
                        <label style={labelStyle}>Year</label>
                        <input value={year} onChange={e => setYear(e.target.value)} type="number" style={inputStyle} />
                    </div>

                    <div style={{ display: "flex", flexDirection: "column", gap: "4px" }}>
                        <label style={labelStyle}>Starring</label>
                        <input value={starring} onChange={e => setStarring(e.target.value)} style={inputStyle} />
                    </div>

                    <div style={{ display: "flex", flexDirection: "column", gap: "4px" }}>
                        <label style={labelStyle}>Genres</label>
                        <div style={{ position: "relative" }}>
                            <input
                                placeholder="Type and press Enter to add"
                                value={genreInput}
                                onChange={e => { setGenreInput(e.target.value); setGenreDropdownOpen(true); }}
                                onKeyDown={handleGenreKeyDown}
                                onFocus={() => setGenreDropdownOpen(true)}
                                onBlur={() => setTimeout(() => setGenreDropdownOpen(false), 150)}
                                style={inputStyle}
                                autoComplete="off"
                            />
                            {genreDropdownOpen && filteredGenres.length > 0 && (
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
                                            style={{ padding: "8px 12px", cursor: "pointer", fontSize: "0.9rem" }}
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
                            <div style={{ display: "flex", flexWrap: "wrap", gap: "6px", marginTop: "4px" }}>
                                {selectedGenres.map(g => (
                                    <span key={g.id} style={{
                                        background: "#333", borderRadius: "20px",
                                        padding: "3px 10px", fontSize: "0.8rem",
                                        display: "flex", alignItems: "center", gap: "6px",
                                    }}>
                                        {g.name}
                                        <button
                                            type="button"
                                            onClick={() => removeGenre(g.id)}
                                            style={{ background: "none", border: "none", color: "#aaa", cursor: "pointer", padding: 0, fontSize: "0.9rem", lineHeight: 1 }}
                                        >×</button>
                                    </span>
                                ))}
                            </div>
                        )}
                    </div>

                    <div style={{ display: "flex", flexDirection: "column", gap: "4px" }}>
                        <label style={labelStyle}>Rating (1–10)</label>
                        <input
                            value={rating}
                            onChange={e => setRating(e.target.value)}
                            type="number" min="1" max="10"
                            placeholder={movieRating !== null ? String(movieRating) : "1–10"}
                            style={inputStyle}
                        />
                    </div>

                    <div style={{ display: "flex", flexDirection: "column", gap: "4px" }}>
                        <label style={labelStyle}>Description</label>
                        <textarea value={description} onChange={e => setDescription(e.target.value)} rows={5} style={{ ...inputStyle, resize: "vertical" }} />
                    </div>

                    {error && <p style={{ color: "red", margin: 0 }}>{error}</p>}
                    {saved && <p style={{ color: "#4caf50", margin: 0 }}>Saved!</p>}

                    <button type="submit" disabled={saving} style={primaryButtonStyle}>
                        {saving ? "Saving..." : "Save changes"}
                    </button>
                </form>
            </div>
        </div>
    );
}

const labelStyle: React.CSSProperties = {
    fontSize: "0.8rem", color: "#aaa", textTransform: "uppercase", letterSpacing: "0.05em",
};

const inputStyle: React.CSSProperties = {
    padding: "8px 12px", borderRadius: "6px", border: "1px solid #333",
    background: "#1a1a1a", color: "#fff", fontSize: "0.95rem",
    width: "100%", boxSizing: "border-box",
};

const primaryButtonStyle: React.CSSProperties = {
    padding: "10px 24px", borderRadius: "6px", background: "#e50914",
    color: "#fff", border: "none", cursor: "pointer", fontWeight: "bold", fontSize: "1rem",
    alignSelf: "flex-start",
};

const secondaryButtonStyle: React.CSSProperties = {
    padding: "8px 16px", borderRadius: "6px", background: "#2a2a2a",
    color: "#fff", border: "none", cursor: "pointer",
};

export default MovieDetails;
