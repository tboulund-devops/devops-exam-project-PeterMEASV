import { Navigate, Outlet, useLocation } from "react-router";
import { useAtomValue } from "jotai";
import { tokenAtom } from "./Token";

export default function RequireAuth() {
    const token = useAtomValue(tokenAtom);
    const location = useLocation();

    if (!token) {
        const next = encodeURIComponent(location.pathname + location.search);
        return <Navigate to={`/login?next=${next}`} replace />;
    }

    return <Outlet />;
}