import { atom } from "jotai";
import type { RouteObject } from "react-router";
import Home from "./Home.tsx";
import Dashboard from "./Dashboard.tsx";
import { LoginPage } from "./LoginPage.tsx";
import RequireAuth from "./RequireAuth.tsx";

export const connectionIdAtom = atom<string | null>(null);

export const routesAtom = atom<RouteObject[]>([
    {
        path: "/",
        element: <Home />
    },
    {
        path: "/login",
        element: <LoginPage />
    },
    {
        element: <RequireAuth />,
        children: [
            {
                path: "/dashboard",
                element: <Dashboard />
            }
        ]
    }
]);