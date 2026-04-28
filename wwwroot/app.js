const tokenKey = "audioClassificationToken";
const userKey = "audioClassificationUser";

function getToken() {
    return localStorage.getItem(tokenKey);
}

function getUsername() {
    return localStorage.getItem(userKey);
}

function setSession(token, username) {
    localStorage.setItem(tokenKey, token);
    localStorage.setItem(userKey, username);
}

function clearSession() {
    localStorage.removeItem(tokenKey);
    localStorage.removeItem(userKey);
}

function redirect(path) {
    window.location.href = path;
}

async function parseResponse(response) {
    const contentType = response.headers.get("content-type") || "";

    if (contentType.includes("application/json")) {
        return await response.json();
    }

    return await response.text();
}

function setMessage(element, message, isError = false) {
    if (!element) {
        return;
    }

    element.textContent = message;
    element.classList.remove("is-success", "is-error");
    element.classList.add(isError ? "is-error" : "is-success");
}

function authHeaders() {
    const token = getToken();
    return token ? { Authorization: `Bearer ${token}` } : {};
}

async function handleRegister() {
    const form = document.getElementById("register-form");
    if (!form) {
        return;
    }

    const message = document.getElementById("register-message");

    form.addEventListener("submit", async (event) => {
        event.preventDefault();

        const payload = {
            username: document.getElementById("register-username").value.trim(),
            email: document.getElementById("register-email").value.trim(), // 🔥 ADD THIS
            password: document.getElementById("register-password").value
        };

        try {
            const response = await fetch("/api/auth/register", {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify(payload)
            });

            const result = await parseResponse(response);

            if (!response.ok) {
                throw new Error(typeof result === "string" ? result : "Registration failed.");
            }

            setMessage(message, "Registration successful. Redirecting to login...");
            window.setTimeout(() => redirect("/login"), 900);
        } catch (error) {
            setMessage(message, error.message || "Registration failed.", true);
        }
    });
}

async function handleLogin() {
    const form = document.getElementById("login-form");
    if (!form) {
        return;
    }

    const message = document.getElementById("login-message");

    if (getToken()) {
        redirect("/dashboard");
        return;
    }

    form.addEventListener("submit", async (event) => {
        event.preventDefault();

        const payload = {
            username: document.getElementById("login-username").value.trim(),
            password: document.getElementById("login-password").value
        };

        try {
            const response = await fetch("/api/auth/login", {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify(payload)
            });

            const result = await parseResponse(response);

            if (!response.ok || !result.token) {
                throw new Error(typeof result === "string" ? result : "Login failed.");
            }

            setSession(result.token, payload.username);
            setMessage(message, "Login successful. Opening dashboard...");
            window.setTimeout(() => redirect("/dashboard"), 500);
        } catch (error) {
            setMessage(message, error.message || "Login failed.", true);
        }
    });
}

function guardDashboard() {
    if (!getToken()) {
        redirect("/login");
        return false;
    }

    const sessionUser = document.getElementById("session-user");
    if (sessionUser) {
        sessionUser.textContent = getUsername() ? `Signed in as ${getUsername()}` : "Authenticated";
    }

    const logoutButton = document.getElementById("logout-button");
    if (logoutButton) {
        logoutButton.addEventListener("click", () => {
            clearSession();
            redirect("/login");
        });
    }

    return true;
}

async function handlePredictForm() {
    const form = document.getElementById("predict-form");
    if (!form) {
        return;
    }

    const resultBox = document.getElementById("prediction-result");

    form.addEventListener("submit", async (event) => {
        event.preventDefault();

        const fileInput = document.getElementById("audio-file");
        const file = fileInput.files[0];

        if (!file) {
            resultBox.innerHTML = "<strong>Prediction</strong><p>Please choose a file first.</p>";
            return;
        }

        const formData = new FormData();
        formData.append("file", file);

        resultBox.innerHTML = "<strong>Prediction</strong><p>Processing audio file...</p>";

        try {
            const response = await fetch("/api/audio/predict", {
                method: "POST",
                headers: authHeaders(),
                body: formData
            });

            const result = await parseResponse(response);

            if (response.status === 401) {
                clearSession();
                redirect("/login");
                return;
            }

            if (!response.ok) {
                throw new Error(typeof result === "string" ? result : "Prediction failed.");
            }

            resultBox.innerHTML = `
                <strong>Prediction</strong>
                <p><b>File:</b> ${result.file}</p>
                <p><b>Label:</b> ${result.prediction}</p>
            `;
        } catch (error) {
            resultBox.innerHTML = `<strong>Prediction</strong><p>${error.message || "Prediction failed."}</p>`;
        }
    });
}

async function handleModelActions() {
    const buttons = document.querySelectorAll(".api-action");
    const output = document.getElementById("action-result");

    if (!buttons.length || !output) {
        return;
    }

    buttons.forEach((button) => {
        button.addEventListener("click", async () => {
            const endpoint = button.dataset.endpoint;
            const label = button.dataset.label || "Action";

            output.innerHTML = `<strong>Response</strong><pre>${label} in progress...</pre>`;

            try {
                const response = await fetch(endpoint, {
                    method: "GET",
                    headers: {
                        ...authHeaders()
                    }
                });

                const result = await parseResponse(response);

                if (response.status === 401) {
                    clearSession();
                    redirect("/login");
                    return;
                }

                if (!response.ok) {
                    throw new Error(typeof result === "string" ? result : JSON.stringify(result, null, 2));
                }

                output.innerHTML = `<strong>Response</strong><pre>${JSON.stringify(result, null, 2)}</pre>`;
            } catch (error) {
                output.innerHTML = `<strong>Response</strong><pre>${error.message || "Request failed."}</pre>`;
            }
        });
    });
}

function init() {
    const page = document.body.dataset.page;

    if (page === "register") {
        handleRegister();
    }

    if (page === "login") {
        handleLogin();
    }

    if (page === "dashboard") {
        const allowed = guardDashboard();
        if (allowed) {
            handlePredictForm();
            handleModelActions();
        }
    }
}

document.addEventListener("DOMContentLoaded", init);
