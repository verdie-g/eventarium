let feed;

export function connect(receiver, url) {
    disconnect();

    feed = new EventSource(url);
    feed.addEventListener("snapshot", event => receiver.invokeMethodAsync("ReceiveMessage", event.data));
    feed.addEventListener("delta", event => receiver.invokeMethodAsync("ReceiveMessage", event.data));
    feed.addEventListener("open", () => receiver.invokeMethodAsync("SetConnectionState", true));
    feed.addEventListener("error", () => receiver.invokeMethodAsync("SetConnectionState", false));
}

export function disconnect() {
    if (feed) {
        feed.close();
        feed = undefined;
    }
}
