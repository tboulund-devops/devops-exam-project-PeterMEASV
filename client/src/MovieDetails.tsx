import { useEffect, useRef, useState } from "react";
import { useLocation, useNavigate } from "react-router";
import { useAtomValue } from "jotai";
import { movieClient } from "./baseUrl.ts";
import type { Movie } from "./generated-ts-client.ts";
import { tokenAtom, userInfoAtom, getUserIdFromToken } from "./Token.tsx";

function MovieDetails() {
    const location = useLocation();
    const navigate = useNavigate();
    const token = useAtomValue(tokenAtom);
    const userId = useAtomValue(userInfoAtom)?.id ?? getUserIdFromToken(token);

    const [movie, setMovie] = useState<Movie | null>(location.state?.movie ?? null);
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

    useEffect(() => {
        if (!movie) return;
        setTitle(movie.title ?? "");
        setYear(String(movie.year ?? ""));
        setDescription(movie.description ?? "");
        setStarring(movie.starring ?? "");
        setPhotoPreview(movie.photo ?? null);
        setRating("");
        movieClient.getMovieRatingByUser(userId ?? undefined, movie.id).then(rating => {
            setMovieRating(rating);
            setRating(String(rating));
        }).catch(() => {
            setMovieRating(null);
        });
    }, [movie]);

    if (!movie) {
        return (
            <div style={{ padding: "32px", color: "#fff" }}>
                <p>Movie not found.</p>
                <button onClick={() => navigate("/dashboard")} style={secondaryButtonStyle}>← Back</button>
            </div>
        );
    }

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
            const updated = await movieClient.editMovie({
                ...movie,
                title: title.trim(),
                year: year ? parseInt(year) as any : movie.year,
                description: description.trim() || undefined,
                starring: starring.trim() || undefined,
            });
            setMovie(updated);

            if (rating && userId && movie.id) {
                await movieClient.updateMovieRating(userId ?? undefined, movie.id, parseInt(rating));
            }

            setSaved(true);
            setTimeout(() => setSaved(false), 2500);
        } catch {
            setError("Could not save changes. Please try again.");
        } finally {
            setSaving(false);
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
            navigate("/dashboard");
        } catch {
            setError("Could not remove movie. Please try again.");
        }
    };

    return (
        <div style={{ minHeight: "100vh", background: "#111", color: "#fff", padding: "32px" }}>
            <button onClick={() => navigate("/dashboard")} style={secondaryButtonStyle}>← Back</button>

            <div style={{
                display: "flex", gap: "40px", marginTop: "28px",
                flexWrap: "wrap",
            }}>
                {/* Poster */}
                <div style={{ display: "flex", flexDirection: "column", gap: "12px", flexShrink: 0 }}>
                    {photoPreview
                        ? <img src={photoPreview} alt={title} style={{ width: "220px", aspectRatio: "2/3", objectFit: "cover", borderRadius: "8px" }} />
                        : <div style={{ width: "220px", aspectRatio: "2/3", background: "#2a2a2a", borderRadius: "8px", display: "flex", alignItems: "center", justifyContent: "center", color: "#666" }}>No poster</div>
                    }
                    <button onClick={() => inputRef.current?.click()} style={secondaryButtonStyle}>Change poster</button>
                    <input ref={inputRef} type="file" accept="image/*" style={{ display: "none" }} onChange={handlePhotoChange} />

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
