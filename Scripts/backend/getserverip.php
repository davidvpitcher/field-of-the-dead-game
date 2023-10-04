<?php // getserverip.php

if (isset($_POST["serverIP"])) {
    $unity = true;
    $login = true;
    
    require_once '../../keys/storage.php'; // Assuming mysqli connection is initialized here as $conn

    try {
        // Use a prepared statement to fetch server settings
        $stmt = $conn->prepare("SELECT `serverip`, `serverport`, `active` FROM `server_settings`");
        $stmt->execute();
        $result = $stmt->get_result();

        if ($result->num_rows != 1) {
            echo "5: failed to login, missing datas";
            exit();
        }

        // Fetch the results into an associative array
        $existinginfo = $result->fetch_assoc();

        echo "0\t" . $existinginfo["serverip"] . "\t" . $existinginfo["serverport"] . "\t" . $existinginfo["active"];
        exit();
    } catch (Exception $e) {
        echo "7: " . $e->getMessage();
        exit();
    }
} else {
    echo "7: uh oh";
    exit();
}
