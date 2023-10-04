<?php
$unity = true; // required for the storage keys to function and get the right keys
$login = true; // required for the storage keys to function and get the right keys
require_once '../../keys/storage.php'; // this grants pdo and conn

if (!isset($_POST['username']) || !isset($_POST['action'])) {
    echo "missing_parameters";
    exit();
}

$username = $_POST['username'];
$action = $_POST['action'];

if ($action == 'get_dailies_status') {
    $sql = "SELECT next_daily FROM dailies WHERE username = ?";
    $stmt = $conn->prepare($sql);
    $stmt->bind_param('s', $username);
    $stmt->execute();
    $result = $stmt->get_result();

    if ($result->num_rows == 0) {
        echo "no_entry";
    } else {
        $row = $result->fetch_assoc();
        echo $row['next_daily'];
    }
} elseif ($action == 'update_dailies') {
    if (!isset($_POST['next_daily'])) {
        echo "missing_next_daily";
        exit();
    }

    $next_daily = $_POST['next_daily'];

    $sql = "SELECT id FROM dailies WHERE username = ?";
    $stmt = $conn->prepare($sql);
    $stmt->bind_param('s', $username);
    $stmt->execute();
    $result = $stmt->get_result();

    if ($result->num_rows == 0) {
        $sql = "INSERT INTO dailies (username, next_daily) VALUES (?, ?)";
        $stmt = $conn->prepare($sql);
        $stmt->bind_param('ss', $username, $next_daily);
    } else {
        $sql = "UPDATE dailies SET next_daily = ? WHERE username = ?";
        $stmt = $conn->prepare($sql);
        $stmt->bind_param('ss', $next_daily, $username);
    }

    $stmt->execute();
    echo "success";
} else {
    echo "invalid_action";
}

$conn->close();
?>
