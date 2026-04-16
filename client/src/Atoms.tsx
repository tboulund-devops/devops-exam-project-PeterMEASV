import { atom } from "jotai";
import type { RouteObject } from "react-router";
import Home from "./Home.tsx";
import Dashboard from "./Dashboard.tsx";
import { LoginPage } from "./LoginPage.tsx";
import RequireAuth from "./RequireAuth.tsx";
import MovieDetails from "./MovieDetails.tsx";
import AppLayout from "./AppLayout.tsx";

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
                element: <AppLayout />,
                children: [
                    {
                        path: "/dashboard",
                        element: <Dashboard />
                    },
                    {
                        path: "/movie/:movieId",
                        element: <MovieDetails />
                    }
                ]
            }
        ]
    }
]);