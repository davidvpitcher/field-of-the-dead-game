<?php // validate_session.inc.php

if (isset($_POST["action"]) && $_POST["action"] === "validateSession" && isset($_POST["sessionId"])) {
    $unity = true; // required to access the keys
    $login = true; // required to access the keys
    require_once '../../keys/storage.php'; // grants pdo and conn
    
    try {
        $sessionId = $_POST["sessionId"];
        
        // Prepare statement to check if this session is active
        $stmt = $conn->prepare("SELECT IsActive FROM PlayerSessions WHERE SessionID=?");
        $stmt->bind_param("s", $sessionId);
        $stmt->execute();
        $result = $stmt->get_result();
        $row = $result->fetch_assoc();

        if ($row && $row["IsActive"]) {
            // Prepare statement to update the LastValidated timestamp
            $stmt = $conn->prepare("UPDATE PlayerSessions SET LastValidated=CURRENT_TIMESTAMP WHERE SessionID=?");
            $stmt->bind_param("s", $sessionId);
            $stmt->execute();

            echo "SESSION_VALID";
        } else {
            echo "SESSION_INVALID";
        }
    } catch (Exception $e) {
        echo "SERVER_ERROR: " . $e->getMessage();
    }
    
    mysqli_close($conn);
} else {
    echo "INVALID_REQUEST";
}
?>
