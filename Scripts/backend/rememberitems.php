<?php // rememberitems.php

if (isset($_POST["username"])) {
    $unity = true;
    $login = true;
    
    $username = $_POST["username"];
    require_once '../../keys/storage.php';
    
    $sql = "SELECT t3.* FROM `player_items` t3
            JOIN `players` t1 ON t1.id = t3.player_id
            WHERE t1.username = :username";

    $stmt = $pdo->prepare($sql);
    $stmt->bindValue(':username', $username);
    $stmt->execute();
    $results = $stmt->fetchAll();

    $totalrow = 0;
    $allvalues = "";

    $sql2 = "SELECT `id`, `item_name` FROM `item`;";
    $stmt2 = $pdo->prepare($sql2);
    $stmt2->execute();
    $arrayofresults = $stmt2->fetchAll();
    
    foreach ($results as $result) {
        $totalrow += 2;
        $id1 = $result['item_id'];
        $itemname = "";
        
        foreach ($arrayofresults as $thinginarray) {
            if ($thinginarray['id'] == $id1) {
                $itemname = $thinginarray['item_name'];
                break;
            }
        }

        $quan = $result['quantity'];
        $allvalues .= "\t" . $itemname . "\t" . $quan;
    }

    echo "0\t" . $totalrow . "\t" . $allvalues;
    exit();
} else {
    echo "7: uh oh";
    exit();
}
