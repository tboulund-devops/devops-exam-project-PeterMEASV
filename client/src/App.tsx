import './App.css'
import {useAtomValue} from "jotai";
import {createBrowserRouter, RouterProvider} from "react-router";
import {routesAtom} from "./Atoms.tsx";
import {useMemo} from "react";

function App() {

    const routes = useAtomValue(routesAtom);
    const router = useMemo(() => createBrowserRouter(routes), [routes]);

    return <RouterProvider router={router} />
}

export default App