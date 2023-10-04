<?php // logout.inc.php

if (isset($_POST["action"]) && $_POST["action"] === "logout" && isset($_POST["sessionID"])) {
    $unity = true;
    $login = true;
    
    require_once '../../keys/storage.php'; // Grants pdo and conn

    try {
        $sessionID = $_POST["sessionID"];

        // Prepare SQL statement to invalidate the session
        $stmt = $conn->prepare("UPDATE PlayerSessions SET IsActive=false WHERE SessionID=?");
        $stmt->bind_param("s", $sessionID);

        if (!$stmt->execute()) {
            echo "DATABASE_ERROR";
            exit();
        }

        echo "SESSION_INVALIDATED";
        mysqli_close($conn);
        exit();
    } catch (Exception $e) {
        echo "SERVER_ERROR: " . $e->getMessage();
        exit();
    }
} else {
    echo "INVALID_REQUEST";
    exit();
}
